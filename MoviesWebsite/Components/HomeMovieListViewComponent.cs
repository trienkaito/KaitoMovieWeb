using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using System.Security.Claims;

namespace MoviesWebsite.Components
{
    public class HomeMovieListViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public HomeMovieListViewComponent(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string filter)
        {
            var movies = await GetMoviesAsync(filter);
            return View(movies);
        }

        private async Task<List<Movie>> GetMoviesAsync(string filter)
        {
            var userId = (User as ClaimsPrincipal)?.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của người dùng hiện tại

            IQueryable<Movie> query = _context.Movies
                .Include(m => m.Categories)
                .Include(m => m.Episodes)
                .Include(m => m.Favourites);

            switch (filter)
            {
                case "Popular":
                    query = query.OrderByDescending(m => m.View);
                    break;
                case "TopRated":
                    query = query.OrderByDescending(m => m.Rate);
                    break;
                case "ComingSoon":
                    query = query.Where(m => !m.Episodes.Any())
                         .OrderByDescending(m => m.CreatedDate);
                    break;
                case "Propose":
                    query = query.Where(m => m.Episodes
                          .Any(e => e.Histories.Any(h => h.UserId == userId)))
                          .Include(m => m.Episodes)
                          .ThenInclude(e => e.Histories)
                          .ThenInclude(h => h.User)
                          .OrderByDescending(m => m.CreatedDate);
                    break;
                default:
                    query = query.OrderByDescending(m => m.CreatedDate);
                    break;
            }

            return await query.Take(12).ToListAsync();
        }
    }
}
