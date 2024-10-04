using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
namespace MoviesWebsite.Components
{ 
    public class PopularSlideViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public PopularSlideViewComponent(AppDbContext context)
        {
            _context = context;
        }
           
        public async Task<IViewComponentResult> InvokeAsync(int size)
        {
            var movies = await GetMoviesAsync(size);
            return View(movies);
        }
        private async Task<List<Movie>> GetMoviesAsync(int size)
        {
            IQueryable<Movie> query = _context.Movies.Include(m => m.Categories)
                                        .OrderByDescending(m => m.View);
            return await query.Take(size).ToListAsync();

        }
    }
}
