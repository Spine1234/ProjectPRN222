using CinemaManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class MoviesController : Controller
    {
        private readonly CinemaManagementContext _context;

        public MoviesController(CinemaManagementContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Director)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();
            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Room)
                .Include(m => m.Genres)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }
    }
}
