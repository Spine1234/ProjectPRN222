using CinemaManagement.Models;
using CinemaManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly CinemaManagementContext _context;

        public AdminController(CinemaManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.MovieCount = await _context.Movies.CountAsync();
            ViewBag.ShowtimeCount = await _context.Showtimes.CountAsync();
            ViewBag.BookingCount = await _context.Bookings.CountAsync();
            ViewBag.CustomerCount = await _context.Customers.CountAsync();
            ViewBag.RoomCount = await _context.Rooms.CountAsync();

            // Lấy 5 đơn đặt vé gần nhất
            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }

        #region Movie Management
        // GET: Admin/Movies
        public async Task<IActionResult> Movies()
        {
            var movies = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.Genres)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(movies);
        }

        // GET: Admin/MovieCreate
        public async Task<IActionResult> MovieCreate()
        {
            // Get directors for dropdown
            ViewBag.Directors = new SelectList(await _context.Directors.OrderBy(d => d.FullName).ToListAsync(), "DirectorId", "FullName");

            // Get genres for multiselect
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            return View(new MovieViewModel());
        }

        // POST: Admin/MovieCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovieCreate(MovieViewModel model, int[] selectedGenres)
        {
            if (ModelState.IsValid)
            {
                var movie = new Movie
                {
                    Title = model.Title,
                    Description = model.Description,
                    DirectorId = model.DirectorId,
                    Duration = model.Duration,
                    ReleaseDate = model.ReleaseDate,
                    EndDate = model.EndDate,
                    PosterUrl = model.PosterUrl,
                    TrailerUrl = model.TrailerUrl,
                    Status = model.Status,
                    Rating = model.Rating,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                // Add selected genres
                if (selectedGenres != null && selectedGenres.Length > 0)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        var genre = await _context.Genres.FindAsync(genreId);
                        if (genre != null)
                        {
                            movie.Genres.Add(genre);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Phim đã được thêm thành công.";
                return RedirectToAction(nameof(Movies));
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Directors = new SelectList(await _context.Directors.OrderBy(d => d.FullName).ToListAsync(), "DirectorId", "FullName");
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return View(model);
        }

        // GET: Admin/MovieEdit/5
        public async Task<IActionResult> MovieEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            var model = new MovieViewModel
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                DirectorId = movie.DirectorId ?? 0,
                Duration = movie.Duration,
                ReleaseDate = movie.ReleaseDate,
                EndDate = movie.EndDate,
                PosterUrl = movie.PosterUrl,
                TrailerUrl = movie.TrailerUrl,
                Status = movie.Status,
                Rating = movie.Rating
            };

            ViewBag.Directors = new SelectList(await _context.Directors.OrderBy(d => d.FullName).ToListAsync(), "DirectorId", "FullName");
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            ViewBag.SelectedGenres = movie.Genres.Select(g => g.GenreId).ToList();

            return View(model);
        }

        // POST: Admin/MovieEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovieEdit(int id, MovieViewModel model, int[] selectedGenres)
        {
            if (id != model.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var movie = await _context.Movies
                        .Include(m => m.Genres)
                        .FirstOrDefaultAsync(m => m.MovieId == id);

                    if (movie == null)
                    {
                        return NotFound();
                    }

                    movie.Title = model.Title;
                    movie.Description = model.Description;
                    movie.DirectorId = model.DirectorId;
                    movie.Duration = model.Duration;
                    movie.ReleaseDate = model.ReleaseDate;
                    movie.EndDate = model.EndDate;
                    movie.PosterUrl = model.PosterUrl;
                    movie.TrailerUrl = model.TrailerUrl;
                    movie.Status = model.Status;
                    movie.Rating = model.Rating;
                    movie.UpdatedAt = DateTime.Now;

                    // Update genres
                    movie.Genres.Clear();
                    if (selectedGenres != null && selectedGenres.Length > 0)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            var genre = await _context.Genres.FindAsync(genreId);
                            if (genre != null)
                            {
                                movie.Genres.Add(genre);
                            }
                        }
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Phim đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Movies));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(model.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Directors = new SelectList(await _context.Directors.OrderBy(d => d.FullName).ToListAsync(), "DirectorId", "FullName");
            ViewBag.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return View(model);
        }

        // GET: Admin/MovieDelete/5
        public async Task<IActionResult> MovieDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Admin/MovieDelete/5
        [HttpPost, ActionName("MovieDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovieDeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            // Check if movie has showtimes
            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.MovieId == id);
            if (hasShowtimes)
            {
                TempData["Error"] = "Không thể xóa phim này vì đã có lịch chiếu.";
                return RedirectToAction(nameof(Movies));
            }

            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Phim đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Movies));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }
        #endregion

        #region Movie Duration Helper
        // GET: Admin/GetMovieDuration/5
        [HttpGet]
        public async Task<IActionResult> GetMovieDuration(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return Json(0);
            }
            return Json(movie.Duration);
        }
        #endregion

        #region Showtime Management
        // GET: Admin/Showtimes
        public async Task<IActionResult> Showtimes()
        {
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            return View(showtimes);
        }

        // GET: Admin/ShowtimeCreate
        public async Task<IActionResult> ShowtimeCreate()
        {
            ViewBag.Movies = new SelectList(await _context.Movies
                .Where(m => m.Status == "Now Showing" || m.Status == "Coming Soon")
                .OrderBy(m => m.Title)
                .ToListAsync(), "MovieId", "Title");

            ViewBag.Rooms = new SelectList(await _context.Rooms
                .Where(r => r.Status == "Available")
                .OrderBy(r => r.Name)
                .ToListAsync(), "RoomId", "Name");

            return View(new ShowtimeViewModel());
        }

        // POST: Admin/ShowtimeCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowtimeCreate(ShowtimeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for conflicting showtimes in the same room
                var conflicts = await CheckShowtimeConflicts(model.RoomId, model.StartTime, model.EndTime);
                if (conflicts)
                {
                    ModelState.AddModelError("", "Đã có lịch chiếu khác trong cùng phòng và thời gian này.");
                    ViewBag.Movies = new SelectList(await _context.Movies.OrderBy(m => m.Title).ToListAsync(), "MovieId", "Title");
                    ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Name).ToListAsync(), "RoomId", "Name");
                    return View(model);
                }

                var showtime = new Showtime
                {
                    MovieId = model.MovieId,
                    RoomId = model.RoomId,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    BasePrice = model.BasePrice,
                    Status = "Scheduled",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Showtimes.Add(showtime);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lịch chiếu đã được thêm thành công.";
                return RedirectToAction(nameof(Showtimes));
            }

            ViewBag.Movies = new SelectList(await _context.Movies.OrderBy(m => m.Title).ToListAsync(), "MovieId", "Title");
            ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Name).ToListAsync(), "RoomId", "Name");
            return View(model);
        }

        // GET: Admin/ShowtimeEdit/5
        public async Task<IActionResult> ShowtimeEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
            {
                return NotFound();
            }

            var model = new ShowtimeViewModel
            {
                ShowtimeId = showtime.ShowtimeId,
                MovieId = showtime.MovieId ?? 0,
                RoomId = showtime.RoomId ?? 0,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                BasePrice = showtime.BasePrice,
                Status = showtime.Status
            };

            ViewBag.Movies = new SelectList(await _context.Movies.OrderBy(m => m.Title).ToListAsync(), "MovieId", "Title");
            ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Name).ToListAsync(), "RoomId", "Name");
            return View(model);
        }

        // POST: Admin/ShowtimeEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowtimeEdit(int id, ShowtimeViewModel model)
        {
            if (id != model.ShowtimeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for conflicting showtimes in the same room (excluding the current showtime)
                    var conflicts = await CheckShowtimeConflicts(model.RoomId, model.StartTime, model.EndTime, model.ShowtimeId);
                    if (conflicts)
                    {
                        ModelState.AddModelError("", "Đã có lịch chiếu khác trong cùng phòng và thời gian này.");
                        ViewBag.Movies = new SelectList(await _context.Movies.OrderBy(m => m.Title).ToListAsync(), "MovieId", "Title");
                        ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Name).ToListAsync(), "RoomId", "Name");
                        return View(model);
                    }

                    var showtime = await _context.Showtimes.FindAsync(id);
                    if (showtime == null)
                    {
                        return NotFound();
                    }

                    showtime.MovieId = model.MovieId;
                    showtime.RoomId = model.RoomId;
                    showtime.StartTime = model.StartTime;
                    showtime.EndTime = model.EndTime;
                    showtime.BasePrice = model.BasePrice;
                    showtime.Status = model.Status;
                    showtime.UpdatedAt = DateTime.Now;

                    _context.Update(showtime);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Lịch chiếu đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Showtimes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShowtimeExists(model.ShowtimeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Movies = new SelectList(await _context.Movies.OrderBy(m => m.Title).ToListAsync(), "MovieId", "Title");
            ViewBag.Rooms = new SelectList(await _context.Rooms.OrderBy(r => r.Name).ToListAsync(), "RoomId", "Name");
            return View(model);
        }

        // GET: Admin/ShowtimeDelete/5
        public async Task<IActionResult> ShowtimeDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ShowtimeId == id);

            if (showtime == null)
            {
                return NotFound();
            }

            return View(showtime);
        }

        // POST: Admin/ShowtimeDelete/5
        [HttpPost, ActionName("ShowtimeDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowtimeDeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);

            // Check if showtime has tickets
            var hasTickets = await _context.Tickets.AnyAsync(t => t.ShowtimeId == id);
            if (hasTickets)
            {
                TempData["Error"] = "Không thể xóa lịch chiếu này vì đã có vé được đặt.";
                return RedirectToAction(nameof(Showtimes));
            }

            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lịch chiếu đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Showtimes));
        }

        private bool ShowtimeExists(int id)
        {
            return _context.Showtimes.Any(e => e.ShowtimeId == id);
        }

        private async Task<bool> CheckShowtimeConflicts(int? roomId, DateTime startTime, DateTime endTime, int? excludeShowtimeId = null)
        {
            // Check if there's any showtime in the same room that overlaps with the given time slot
            var query = _context.Showtimes
                .Where(s => s.RoomId == roomId &&
                           ((s.StartTime <= startTime && s.EndTime > startTime) || // Existing showtime starts before and ends after new start
                            (s.StartTime < endTime && s.EndTime >= endTime) || // Existing showtime starts before and ends after new end
                            (s.StartTime >= startTime && s.EndTime <= endTime))); // Existing showtime is completely within new showtime

            if (excludeShowtimeId.HasValue)
            {
                query = query.Where(s => s.ShowtimeId != excludeShowtimeId.Value);
            }

            return await query.AnyAsync();
        }
        #endregion

        #region Room Management
        // GET: Admin/Rooms
        public async Task<IActionResult> Rooms()
        {
            var rooms = await _context.Rooms
                .OrderBy(r => r.Name)
                .ToListAsync();

            return View(rooms);
        }

        // GET: Admin/RoomCreate
        public IActionResult RoomCreate()
        {
            return View(new RoomViewModel());
        }

        // POST: Admin/RoomCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoomCreate(RoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                var room = new Room
                {
                    Name = model.Name,
                    Capacity = model.Capacity,
                    Description = model.Description,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Phòng chiếu đã được thêm thành công.";
                return RedirectToAction(nameof(Rooms));
            }

            return View(model);
        }

        // GET: Admin/RoomEdit/5
        public async Task<IActionResult> RoomEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var model = new RoomViewModel
            {
                RoomId = room.RoomId,
                Name = room.Name,
                Capacity = room.Capacity,
                Description = room.Description,
                Status = room.Status
            };

            return View(model);
        }

        // POST: Admin/RoomEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoomEdit(int id, RoomViewModel model)
        {
            if (id != model.RoomId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var room = await _context.Rooms.FindAsync(id);
                    if (room == null)
                    {
                        return NotFound();
                    }

                    room.Name = model.Name;
                    room.Capacity = model.Capacity;
                    room.Description = model.Description;
                    room.Status = model.Status;
                    room.UpdatedAt = DateTime.Now;

                    _context.Update(room);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Phòng chiếu đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Rooms));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(model.RoomId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // GET: Admin/RoomDelete/5
        public async Task<IActionResult> RoomDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Admin/RoomDelete/5
        [HttpPost, ActionName("RoomDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RoomDeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            // Check if room has showtimes
            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.RoomId == id);
            if (hasShowtimes)
            {
                TempData["Error"] = "Không thể xóa phòng chiếu này vì đã có lịch chiếu sử dụng.";
                return RedirectToAction(nameof(Rooms));
            }

            // Check if room has seats
            var hasSeats = await _context.Seats.AnyAsync(s => s.RoomId == id);
            if (hasSeats)
            {
                TempData["Error"] = "Không thể xóa phòng chiếu này vì đã có ghế được tạo.";
                return RedirectToAction(nameof(Rooms));
            }

            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Phòng chiếu đã được xóa thành công.";
            }

            return RedirectToAction(nameof(Rooms));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }

        // GET: Admin/RoomDetails/5
        public async Task<IActionResult> RoomDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Seats)
                    .ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            ViewBag.SeatTypes = await _context.SeatTypes.ToListAsync();

            return View(room);
        }

        // POST: Admin/AddSeat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSeat(int roomId, string rowNumber, int seatNumber, int seatTypeId)
        {
            // Check if seat already exists
            var seatExists = await _context.Seats
                .AnyAsync(s => s.RoomId == roomId && s.RowNumber == rowNumber && s.SeatNumber == seatNumber);

            if (seatExists)
            {
                TempData["Error"] = "Ghế này đã tồn tại trong phòng.";
                return RedirectToAction(nameof(RoomDetails), new { id = roomId });
            }

            var seat = new Seat
            {
                RoomId = roomId,
                SeatTypeId = seatTypeId,
                RowNumber = rowNumber,
                SeatNumber = seatNumber,
                Status = "Available",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ghế đã được thêm thành công.";
            return RedirectToAction(nameof(RoomDetails), new { id = roomId });
        }

        // POST: Admin/RemoveSeat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSeat(int seatId, int roomId)
        {
            var seat = await _context.Seats.FindAsync(seatId);

            // Check if seat has tickets
            var hasTickets = await _context.Tickets.AnyAsync(t => t.SeatId == seatId);
            if (hasTickets)
            {
                TempData["Error"] = "Không thể xóa ghế này vì đã có vé được đặt.";
                return RedirectToAction(nameof(RoomDetails), new { id = roomId });
            }

            if (seat != null)
            {
                _context.Seats.Remove(seat);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ghế đã được xóa thành công.";
            }

            return RedirectToAction(nameof(RoomDetails), new { id = roomId });
        }
        #endregion

        #region Report Management
        // Phương thức thống kê doanh thu
        public async Task<IActionResult> Revenue()
        {
            // Lấy doanh thu theo ngày trong 30 ngày gần nhất
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var dailyRevenue = await _context.Bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate && b.PaymentStatus == "Completed")
                .GroupBy(b => b.BookingDate.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Thống kê doanh thu theo tháng trong năm hiện tại
            var currentYear = DateTime.Now.Year;
            var monthlyRevenue = await _context.Bookings
                .Where(b => b.BookingDate.Value.Year == currentYear && b.PaymentStatus == "Completed")
                .GroupBy(b => b.BookingDate.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            // Thống kê doanh thu theo phương thức thanh toán
            var paymentMethodRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == "Completed")
                .GroupBy(b => b.PaymentMethod)
                .Select(g => new
                {
                    PaymentMethod = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            // Tổng doanh thu
            var totalRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == "Completed")
                .SumAsync(b => b.TotalAmount);

            // Số lượng đơn đặt hàng
            var totalBookings = await _context.Bookings
                .Where(b => b.PaymentStatus == "Completed")
                .CountAsync();

            // Giá trị đơn hàng trung bình
            var averageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0;

            ViewBag.DailyRevenue = dailyRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.PaymentMethodRevenue = paymentMethodRevenue;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.AverageBookingValue = averageBookingValue;

            return View();
        }

        // Phương thức báo cáo lượng khách
        public async Task<IActionResult> CustomerReport()
        {
            // Tổng số khách hàng
            var totalCustomers = await _context.Customers.CountAsync();

            // Khách hàng mới trong 30 ngày qua
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var newCustomers = await _context.Customers
                .Where(c => c.CreatedAt >= thirtyDaysAgo)
                .CountAsync();

            // Khách hàng hoạt động (có đặt vé trong 30 ngày qua)
            var activeCustomers = await _context.Bookings
                .Where(b => b.BookingDate >= thirtyDaysAgo)
                .Select(b => b.CustomerId)
                .Distinct()
                .CountAsync();

            // Top 10 khách hàng có nhiều đơn đặt hàng nhất
            var topCustomersByOrders = await _context.Bookings
                .GroupBy(b => b.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    BookingCount = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(10)
                .ToListAsync();

            // Top 10 khách hàng chi tiêu nhiều nhất
            var topCustomersBySpending = await _context.Bookings
                .GroupBy(b => b.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    BookingCount = g.Count(),
                    TotalSpent = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            // Lấy thông tin chi tiết của khách hàng
            var customerIds = topCustomersByOrders.Select(c => c.CustomerId)
                .Union(topCustomersBySpending.Select(c => c.CustomerId))
                .Distinct()
                .ToList();

            var customerDetails = await _context.Customers
                .Where(c => customerIds.Contains(c.CustomerId))
                .ToListAsync();

            // Chuẩn bị dữ liệu cho view
            var topOrdersWithDetails = topCustomersByOrders.Select(c => new
            {
                c.CustomerId,
                c.BookingCount,
                c.TotalSpent,
                CustomerName = customerDetails.FirstOrDefault(cd => cd.CustomerId == c.CustomerId)?.FullName ?? $"Khách hàng #{c.CustomerId}",
                CustomerEmail = customerDetails.FirstOrDefault(cd => cd.CustomerId == c.CustomerId)?.Email
            }).ToList();

            var topSpendingWithDetails = topCustomersBySpending.Select(c => new
            {
                c.CustomerId,
                c.BookingCount,
                c.TotalSpent,
                CustomerName = customerDetails.FirstOrDefault(cd => cd.CustomerId == c.CustomerId)?.FullName ?? $"Khách hàng #{c.CustomerId}",
                CustomerEmail = customerDetails.FirstOrDefault(cd => cd.CustomerId == c.CustomerId)?.Email
            }).ToList();

            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.NewCustomers = newCustomers;
            ViewBag.ActiveCustomers = activeCustomers;
            ViewBag.TopCustomersByOrders = topOrdersWithDetails;
            ViewBag.TopCustomersBySpending = topSpendingWithDetails;

            return View();
        }

        // Phương thức báo cáo phim ăn khách nhất
        public async Task<IActionResult> PopularMovies()
        {
            try
            {
                // Lấy phim bán nhiều vé nhất
                var topMoviesByTickets = await _context.Tickets
                    .Include(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                    .Where(t => t.Status != "Cancelled" && t.Showtime.Movie != null)
                    .GroupBy(t => t.Showtime.MovieId)
                    .Select(g => new
                    {
                        MovieId = g.Key,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(t => t.Price),
                        Movie = g.FirstOrDefault().Showtime.Movie
                    })
                    .OrderByDescending(x => x.TicketCount)
                    .Take(10)
                    .ToListAsync();

                // Lấy phim có doanh thu cao nhất
                var topMoviesByRevenue = await _context.Tickets
                    .Include(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                    .Where(t => t.Status != "Cancelled" && t.Showtime.Movie != null)
                    .GroupBy(t => t.Showtime.MovieId)
                    .Select(g => new
                    {
                        MovieId = g.Key,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(t => t.Price),
                        Movie = g.FirstOrDefault().Showtime.Movie
                    })
                    .OrderByDescending(x => x.Revenue)
                    .Take(10)
                    .ToListAsync();

                // Thống kê theo thể loại
                var genreStats = await _context.Tickets
                    .Include(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                            .ThenInclude(m => m.Genres)
                    .Where(t => t.Status != "Cancelled" && t.Showtime.Movie != null && t.Showtime.Movie.Genres.Any())
                    .SelectMany(t => t.Showtime.Movie.Genres.Select(g => new { GenreId = g.GenreId, GenreName = g.Name, Price = t.Price }))
                    .GroupBy(x => new { x.GenreId, x.GenreName })
                    .Select(g => new
                    {
                        GenreId = g.Key.GenreId,
                        GenreName = g.Key.GenreName,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(x => x.Price)
                    })
                    .OrderByDescending(x => x.TicketCount)
                    .ToListAsync();

                // Gán trực tiếp cho ViewBag
                ViewBag.TopMoviesByTickets = topMoviesByTickets;
                ViewBag.TopMoviesByRevenue = topMoviesByRevenue;
                ViewBag.GenreStats = genreStats;

                // Kiểm tra có dữ liệu không
                ViewBag.HasData = (
                    (topMoviesByTickets != null && topMoviesByTickets.Any()) ||
                    (topMoviesByRevenue != null && topMoviesByRevenue.Any()) ||
                    (genreStats != null && genreStats.Any())
                );

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Lỗi khi tạo báo cáo phim ăn khách: {ex.Message}");

                // Cung cấp các danh sách rỗng cho view
                ViewBag.TopMoviesByTickets = new List<object>();
                ViewBag.TopMoviesByRevenue = new List<object>();
                ViewBag.GenreStats = new List<object>();
                ViewBag.HasData = false;
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải dữ liệu báo cáo phim. Vui lòng thử lại sau.";

                return View();
            }
        }

        // Báo cáo thống kê theo ngày
        public async Task<IActionResult> DailyReport(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Mặc định là ngày hôm nay nếu không có giá trị
                var today = DateTime.Today;
                var start = startDate ?? today.AddDays(-30);
                var end = endDate ?? today;

                // Đảm bảo ngày bắt đầu nhỏ hơn ngày kết thúc
                if (start > end)
                {
                    var temp = start;
                    start = end;
                    end = temp;
                }

                // Lấy danh sách vé theo ngày
                var dailyTickets = await _context.Tickets
                    .Include(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                    .Include(t => t.Booking)
                    .Where(t => t.Status != "Cancelled" &&
                            t.Showtime != null &&
                            t.Showtime.StartTime.Date >= start.Date &&
                            t.Showtime.StartTime.Date <= end.Date)
                    .GroupBy(t => t.Showtime.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(t => t.Price),
                        ShowtimeCount = g.Select(t => t.ShowtimeId).Distinct().Count(),
                        MovieCount = g.Select(t => t.Showtime.MovieId).Distinct().Count(),
                        CustomerCount = g.Select(t => t.Booking.CustomerId).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                // Tính tổng số liệu
                var totalTickets = dailyTickets.Sum(d => d.TicketCount);
                var totalRevenue = dailyTickets.Sum(d => d.Revenue);
                var avgDailyRevenue = dailyTickets.Any() ? totalRevenue / dailyTickets.Count : 0;

                // Top phim bán chạy trong khoảng thời gian
                var topMovies = await _context.Tickets
                    .Include(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                    .Where(t => t.Status != "Cancelled" &&
                            t.Showtime != null &&
                            t.Showtime.StartTime.Date >= start.Date &&
                            t.Showtime.StartTime.Date <= end.Date)
                    .GroupBy(t => new { t.Showtime.MovieId, t.Showtime.Movie.Title })
                    .Select(g => new
                    {
                        MovieId = g.Key.MovieId,
                        Title = g.Key.Title,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(t => t.Price)
                    })
                    .OrderByDescending(x => x.TicketCount)
                    .Take(5)
                    .ToListAsync();

                // Chuẩn bị dữ liệu cho biểu đồ - PHẦN THÊM VÀO
                string dates = "";
                string ticketCounts = "";
                string revenues = "";

                if (dailyTickets.Any())
                {
                    dates = string.Join(",", dailyTickets.Select(d => $"'{d.Date.ToString("dd/MM")}'"));
                    ticketCounts = string.Join(",", dailyTickets.Select(d => d.TicketCount));
                    revenues = string.Join(",", dailyTickets.Select(d => d.Revenue));
                }

                // Gán dữ liệu cho ViewBag
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                ViewBag.DailyTickets = dailyTickets;
                ViewBag.TotalTickets = totalTickets;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.AverageDailyRevenue = avgDailyRevenue;
                ViewBag.TopMovies = topMovies;

                // Thêm các biến cho biểu đồ
                ViewBag.Dates = dates;
                ViewBag.TicketCounts = ticketCounts;
                ViewBag.Revenues = revenues;

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Lỗi khi tạo báo cáo theo ngày: {ex.Message}");
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tạo báo cáo. Vui lòng thử lại sau.";
                return View();
            }
        }

        // Báo cáo thống kê theo tháng
        public async Task<IActionResult> MonthlyReport(int? year)
        {
            try
            {
                // Mặc định là năm hiện tại nếu không có giá trị
                var currentYear = year ?? DateTime.Today.Year;

                // Lấy dữ liệu vé theo tháng trong năm
                var monthlyTickets = await _context.Tickets
                    .Include(t => t.Showtime)
                    .Include(t => t.Booking)
                    .Where(t => t.Status != "Cancelled" &&
                            t.Showtime != null &&
                            t.Showtime.StartTime.Year == currentYear)
                    .GroupBy(t => t.Showtime.StartTime.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(t => t.Price),
                        ShowtimeCount = g.Select(t => t.ShowtimeId).Distinct().Count(),
                        MovieCount = g.Select(t => t.Showtime.MovieId).Distinct().Count(),
                        CustomerCount = g.Select(t => t.Booking.CustomerId).Distinct().Count()
                    })
                    .OrderBy(x => x.Month)
                    .ToListAsync();

                // Lấy danh sách các năm có dữ liệu để hiển thị dropdown
                var availableYears = await _context.Tickets
                    .Include(t => t.Showtime)
                    .Where(t => t.Showtime != null)
                    .Select(t => t.Showtime.StartTime.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                // Tính tổng số liệu
                var totalTickets = monthlyTickets.Sum(m => m.TicketCount);
                var totalRevenue = monthlyTickets.Sum(m => m.Revenue);

                // Top thể loại bán chạy trong năm
                var topGenres = await _context.Tickets
                    .Include(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                            .ThenInclude(m => m.Genres)
                    .Where(t => t.Status != "Cancelled" &&
                            t.Showtime != null &&
                            t.Showtime.StartTime.Year == currentYear)
                    .SelectMany(t => t.Showtime.Movie.Genres.Select(g => new { GenreId = g.GenreId, GenreName = g.Name, Price = t.Price }))
                    .GroupBy(x => new { x.GenreId, x.GenreName })
                    .Select(g => new
                    {
                        GenreId = g.Key.GenreId,
                        GenreName = g.Key.GenreName,
                        TicketCount = g.Count(),
                        Revenue = g.Sum(x => x.Price)
                    })
                    .OrderByDescending(x => x.TicketCount)
                    .Take(5)
                    .ToListAsync();

                ViewBag.Year = currentYear;
                ViewBag.AvailableYears = availableYears;
                ViewBag.MonthlyTickets = monthlyTickets;
                ViewBag.TotalTickets = totalTickets;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.TopGenres = topGenres;

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Lỗi khi tạo báo cáo theo tháng: {ex.Message}");

                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tạo báo cáo. Vui lòng thử lại sau.";
                return View();
            }
        }
        #endregion
    }
}