using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CinemaManagement.Models;

namespace CinemaManagement.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class ProfileController : Controller
    {
        private readonly CinemaManagementContext _context;

        public ProfileController(CinemaManagementContext context)
        {
            _context = context;
        }

        // GET: Profile - Trang tổng quan
        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(customer);
        }

        // GET: Profile/BookingHistory - Lịch sử đặt vé
        public async Task<IActionResult> BookingHistory()
        {
            var username = User.Identity.Name;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Seat)
                .Where(b => b.CustomerId == customer.CustomerId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Profile/Edit - Chỉnh sửa thông tin cá nhân
        public async Task<IActionResult> Edit()
        {
            var username = User.Identity.Name;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(customer);
        }

        // POST: Profile/Edit - Xử lý chỉnh sửa thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            var username = User.Identity.Name;
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (existingCustomer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin, giữ nguyên các trường không được phép sửa
                existingCustomer.FullName = customer.FullName;
                existingCustomer.Email = customer.Email;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.DateOfBirth = customer.DateOfBirth;
                existingCustomer.Address = customer.Address;
                existingCustomer.UpdatedAt = DateTime.Now;

                // Cập nhật mật khẩu nếu đã nhập
                if (!string.IsNullOrEmpty(customer.PasswordHash))
                {
                    existingCustomer.PasswordHash = customer.PasswordHash;
                }

                _context.Update(existingCustomer);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Thông tin cá nhân đã được cập nhật";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: Profile/ViewTicket/5 - Xem chi tiết vé
        public async Task<IActionResult> ViewTicket(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var username = User.Identity.Name;
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Room)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(t => t.TicketId == id && t.Booking.CustomerId == customer.CustomerId);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
    }
}