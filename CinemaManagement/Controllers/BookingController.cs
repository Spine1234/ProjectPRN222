using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Models;
using System.Security.Claims;
using System.Linq;

namespace CinemaManagement.Controllers
{
    public class BookingController : Controller
    {
        private readonly CinemaManagementContext _context;

        public BookingController(CinemaManagementContext context)
        {
            _context = context;
        }

        // GET: Booking/Index/5 (Movie ID)
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.Genres)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Room)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Chỉ lấy các lịch chiếu trong tương lai, sắp xếp theo thời gian
            var upcomingShowtimes = movie.Showtimes
                .Where(s => s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .ToList();

            ViewBag.Movie = movie;
            ViewBag.Showtimes = upcomingShowtimes;

            return View();
        }

        // GET: Booking/SelectSeats/5 (Showtime ID)
        [Authorize]
        public async Task<IActionResult> SelectSeats(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            // Xóa cache dữ liệu
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var showtime = await _context.Showtimes
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .Include(s => s.Room.Seats)
                    .ThenInclude(seat => seat.SeatType)
                .FirstOrDefaultAsync(s => s.ShowtimeId == id);

            if (showtime == null)
            {
                return NotFound();
            }

            // Force lấy mới nhất từ DB - không dùng cache
            _context.Database.ExecuteSqlRaw("DBCC DROPCLEANBUFFERS"); // Xóa cache SQL Server

            var bookedSeats = await _context.Tickets
                .AsNoTracking() // Không theo dõi thay đổi
                .Where(t => t.ShowtimeId == id && t.Status != "Cancelled")
                .Select(t => t.SeatId)
                .ToListAsync();

            ViewBag.Showtime = showtime;
            ViewBag.BookedSeats = bookedSeats;

            // Debug info - log số ghế đã đặt
            Console.WriteLine($"ShowtimeId: {id}, Số ghế đã đặt: {bookedSeats.Count}");
            foreach (var seatId in bookedSeats)
            {
                Console.WriteLine($"   SeatId: {seatId}");
            }

            return View();
        }


        // POST: Booking/SelectSeats
        [HttpPost]
        [Authorize]
        public IActionResult SelectSeats(int showtimeId, List<int> selectedSeats)
        {
            if (selectedSeats == null || !selectedSeats.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một ghế";
                return RedirectToAction("SelectSeats", new { id = showtimeId });
            }

            // Lưu thông tin tạm thời vào TempData để sử dụng trong Confirm
            TempData["ShowtimeId"] = showtimeId;
            TempData["SelectedSeats"] = string.Join(",", selectedSeats);

            return RedirectToAction("Confirm");
        }

        // GET: Booking/Confirm
        [Authorize]
        public async Task<IActionResult> Confirm()
        {
            if (TempData["ShowtimeId"] == null || TempData["SelectedSeats"] == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            int showtimeId = (int)TempData["ShowtimeId"];
            string seatsString = TempData["SelectedSeats"].ToString();

            // Giữ lại giá trị để có thể truy cập trong POST
            TempData.Keep("ShowtimeId");
            TempData.Keep("SelectedSeats");

            var selectedSeats = seatsString.Split(',').Select(int.Parse).ToList();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId);

            var seats = await _context.Seats
                .Include(s => s.SeatType)
                .Where(s => selectedSeats.Contains(s.SeatId))
                .ToListAsync();

            // Tính tổng tiền
            decimal totalAmount = 0;
            foreach (var seat in seats)
            {
                totalAmount += showtime.BasePrice * seat.SeatType.PriceMultiplier;
            }

            ViewBag.Showtime = showtime;
            ViewBag.Seats = seats;
            ViewBag.TotalAmount = totalAmount;

            return View();
        }

        // POST: Booking/Confirm
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Confirm(string paymentMethod)
        {
            if (TempData["ShowtimeId"] == null || TempData["SelectedSeats"] == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            int showtimeId = (int)TempData["ShowtimeId"];
            string seatsString = TempData["SelectedSeats"].ToString();
            var selectedSeats = seatsString.Split(',').Select(int.Parse).ToList();

            // Kiểm tra xem ghế đã được đặt chưa
            var bookedSeats = await _context.Tickets
                .Where(t => t.ShowtimeId == showtimeId &&
                       selectedSeats.Any(s => s == t.SeatId) &&
                       t.Status != "Cancelled")
                .ToListAsync();

            if (bookedSeats.Any())
            {
                // Có ghế đã bị đặt, thông báo lỗi
                TempData["Error"] = "Một số ghế đã được đặt bởi người khác. Vui lòng chọn ghế khác.";
                return RedirectToAction("SelectSeats", new { id = showtimeId });
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId);

            var seats = await _context.Seats
                .Include(s => s.SeatType)
                .Where(s => selectedSeats.Contains(s.SeatId))
                .ToListAsync();

            // Tính tổng tiền
            decimal totalAmount = 0;
            foreach (var seat in seats)
            {
                totalAmount += showtime.BasePrice * seat.SeatType.PriceMultiplier;
            }

            // Lấy thông tin khách hàng hiện tại
            string username = User.Identity.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Thực hiện đặt vé trong transaction để đảm bảo tính toàn vẹn dữ liệu
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Tạo booking
                    var booking = new Booking
                    {
                        CustomerId = customer.CustomerId,
                        BookingDate = DateTime.Now,
                        TotalAmount = totalAmount,
                        PaymentMethod = paymentMethod,
                        PaymentStatus = "Completed",
                        BookingStatus = "Confirmed",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    // Tạo tickets
                    foreach (var seat in seats)
                    {
                        var price = showtime.BasePrice * seat.SeatType.PriceMultiplier;
                        var ticketCode = "TK" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + seat.SeatId;

                        var ticket = new Ticket
                        {
                            BookingId = booking.BookingId,
                            ShowtimeId = showtimeId,
                            SeatId = seat.SeatId,
                            Price = price,
                            TicketCode = ticketCode,
                            Status = "Active",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        _context.Tickets.Add(ticket);
                    }

                    // Cập nhật điểm tích lũy cho khách hàng
                    customer.MembershipPoints += (int)(totalAmount / 10000); // Cứ 10.000 VND được 1 điểm
                    _context.Customers.Update(customer);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction("Success", new { id = booking.BookingId });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Đã xảy ra lỗi khi đặt vé. Vui lòng thử lại.";
                    return RedirectToAction("SelectSeats", new { id = showtimeId });
                }
            }
        }

        // GET: Booking/Success/5
        [Authorize]
        public async Task<IActionResult> Success(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Movies");
            }

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Showtime)
                        .ThenInclude(s => s.Room)  // Đảm bảo Room được load
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Seat)
                        .ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookedSeats(int showtimeId)
        {
            var bookedSeats = await _context.Tickets
                .Where(t => t.ShowtimeId == showtimeId && t.Status != "Cancelled")
                .Select(t => t.SeatId)
                .ToListAsync();

            return Json(bookedSeats);
        }
    }
}