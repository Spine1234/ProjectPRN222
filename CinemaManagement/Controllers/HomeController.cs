using CinemaManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CinemaManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaManagementContext _context;

        public HomeController(CinemaManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var nowShowingMovies = await _context.Movies
                .Where(m => m.Status == "Now Showing")
                .Include(m => m.Director)
                .OrderByDescending(m => m.ReleaseDate)
                .Take(6)
                .ToListAsync();

            return View(nowShowingMovies);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

}
