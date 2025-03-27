using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaManagement.Models;

namespace CinemaManagement.ViewComponents
{
    public class RecentBookingsViewComponent : ViewComponent
    {
        private readonly CinemaManagementContext _context;

        public RecentBookingsViewComponent(CinemaManagementContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int customerId, int count = 3)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tickets)
                    .ThenInclude(t => t.Showtime)
                        .ThenInclude(s => s.Movie)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BookingDate)
                .Take(count)
                .ToListAsync();

            return View(bookings);
        }
    }
}