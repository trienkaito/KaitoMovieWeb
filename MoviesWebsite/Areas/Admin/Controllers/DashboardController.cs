using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;

namespace MoviesWebsite.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("/admin/dashboard/{action=index}/{id?}")]
    [Authorize(Policy = "Edit")]
    public class DashboardController : Controller
    {

        // GET: DashboardController
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            var topMovies = _context.Movies
                .Where(m => m.View > 0)
                .OrderBy(m => m.View)
                .Take(6)
                .Select(m => new Movie
                {
                    Title = m.Title,
                    View = m.View
                }).ToList();


            ViewData["Movies"] = _context.Movies.Count();
            ViewData["Categories"] = _context.Categories.Count();
            ViewData["Actors"] = _context.Actors.Count();
            ViewData["Comments"] = _context.Comments.Count();

            ViewData["MoviesTitle"] = topMovies.Select(m => m.Title).ToList();
            ViewData["MoviesView"] = topMovies.Select(m => m.View);
            return View();
        }
    }
}
