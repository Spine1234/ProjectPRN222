using CinemaManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            // Lấy 5 đơn đặt vé gần nhất
            var recentBookings = await _context.Bookings
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            return View(recentBookings);
        }

    }
}
