using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;

namespace MoviesWebsite.Components
{
    public class ActorViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ActorViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(string filter)
        {
            var movies = await GetPopularActorAsync(filter);
            return View(movies);
        }

        private async Task<List<Actor>> GetPopularActorAsync(string filter)
        {
            var query = _context.Actors
                 .Include(a => a.Movies)
                 .Select(a => new { actor = a, count = a.Movies.Count });
            var actor = await query.OrderByDescending(a => a.count).Select(a => a.actor).Take(5).ToListAsync();

            return actor;
        }

    }
}
