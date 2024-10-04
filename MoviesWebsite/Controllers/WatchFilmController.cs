using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using System.Security.Claims;

namespace MoviesWebsite.Controllers
{
    [Route("WatchFilm/{action=Index}/{slug?}")]
    public class WatchFilmController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WatchFilmController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class WatchMovie
        {
            public string Link { get; set; }
            public Movie Movie { get; set; }
        }

        public async Task<IActionResult> Index(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var episode = await _context.Episodes.Include(e => e.Movie)
                .FirstOrDefaultAsync(e => e.Slug == slug);

            if (episode == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Comments)
                .ThenInclude(c => c.User)
                .Include(m => m.Episodes)
                .FirstOrDefaultAsync(m => m.MovieId == episode.MovieId);

            if (movie == null)
            {
                return NotFound(); 
            }

            var userId = _userManager.GetUserId(this.User);
            ViewBag.UserId = userId;

            // Kiểm tra lịch sử xem
            var watchHistory = await _context.Histories
                .FirstOrDefaultAsync(h => userId != null && h.UserId == userId && h.EpisodeId == episode.EpisodeId);

            if (watchHistory != null)
            {
                if (watchHistory.Time?.AddHours(2) >= DateTime.Now)
                {
                    // Tăng tổng số view lên 1
                    movie.View++;
                }
                watchHistory.Time = DateTime.Now;
            }
            else
            {
                // Tăng tổng số view lên 1
                movie.View++;

                if(userId != null)
                {
                    _context.Histories.Add(new History
                    {
                        UserId = userId,
                        EpisodeId = episode.EpisodeId,
                        Time = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();

            var model = new WatchMovie()
            {
                Movie = movie,
                Link = episode.Link
            };
            return View(model);
        }

        // Action xử lý thay đổi tập phim
        [HttpPost]
        public async Task<IActionResult> ChangeEpisode(string slug)
        {
           if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            return RedirectToAction("Index", "WatchFilm",new {slug = slug});
        }
    }
}
