using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using MVC.Models;
using static MoviesWebsite.Controllers.ViewMoviesController;

namespace MoviesWebsite.Controllers
{
	[Authorize]
	[Route("/favourite/{action=Index}")]
    public class FavouritesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

		public FavouritesController(AppDbContext context, UserManager<AppUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: Favourites
		public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage,
					string searchString,
					int pageSize,
					List<int> categories,
					List<int> actor,
					List<int> director,
					int fromYear,
					int toYear,
					int sortBy,
					string view)
		{
			var viewModel = new ViewCategory();
			var userid = _userManager.GetUserId(this.User);

			ViewBag.Categories = new MultiSelectList(_context.Categories.OrderBy(c => c.Title),
						nameof(Category.CategoryId),
						nameof(Category.Title));

			ViewBag.Actors = new MultiSelectList(_context.Actors
							.Where(a => a.Role.Contains("Actor"))
							.OrderBy(a => a.Name),
						nameof(Actor.ActorId),
						nameof(Actor.Name));

			ViewBag.Directors = new SelectList(_context.Actors
							.Where(a => a.Role.Contains("Director"))
							.OrderBy(a => a.Name),
						nameof(Actor.ActorId),
						nameof(Actor.Name));

			ViewBag.FromYear = new SelectList(_context.Movies
							.Where(m => m.ReleaseDate != null
							&& m.Favourites != null
							&& m.Favourites.Where(f => f.UserId == userid).Count() > 0)
							.Select(m => m.ReleaseDate.Value.Year)
							.Distinct()
							.OrderBy(i => i));

			ViewBag.ToYear = ViewBag.FromYear;
			var sortBys = new List<SortBy>()
			{
				new SortBy() {Id = 1, Name = "Popularity Descending"},
				new SortBy() {Id = 2, Name = "Popularity Ascending"},
				new SortBy() {Id = 3, Name = "Rating Descending"},
				new SortBy() {Id = 4, Name = "Rating Ascending"},
				new SortBy() {Id = 5, Name = "Release date Descending"},
				new SortBy() {Id = 6, Name = "Release date Ascending"},
				new SortBy() {Id = 7, Name = "A-Z"},
				new SortBy() {Id = 8, Name = "Z-A"},

			};
			ViewBag.SortBy = new SelectList(sortBys, nameof(SortBy.Id), nameof(SortBy.Name));

			var moviesPerPages = new List<MoviesPerPage>()
			{
				new MoviesPerPage() {PageSize=10, Name = "10 Movies"},
				new MoviesPerPage() {PageSize=20, Name = "20 Movies"}
			};
			ViewBag.MoviesPerPages = new SelectList(moviesPerPages, nameof(MoviesPerPage.PageSize), nameof(MoviesPerPage.Name));


			// filter
			// category
			if (categories != null && categories.Count > 0)
			{
				viewModel.Categories = await (from c in _context.Categories
											  join i in categories on c.CategoryId equals i
											  select c).ToListAsync();
			}

			if (viewModel.Categories != null && viewModel.Categories.Count > 0)
			{
				viewModel.Movies = new List<Movie>();
				foreach (var m in _context.Movies.Include(m => m.Actors).Include(m => m.Categories).Include(m => m.Favourites))
				{
					if (viewModel.Categories.All(c => m.Categories != null && m.Categories.Contains(c)))
					{
						viewModel.Movies.Add(m);
					}
				}
			}
			else
			{
				viewModel.Movies = await _context.Movies.Include(m => m.Actors).Include(m => m.Categories).Include(m => m.Favourites).ToListAsync();
			}

			//favorite
			viewModel.Movies = viewModel.Movies.Where( m => m.Favourites != null
				&&  m.Favourites.Where(f => f.UserId == userid).Count() > 0)
				.ToList();

			// actor
			if (actor != null && actor.Count > 0)
			{
				var actors = await (from a in _context.Actors
									join i in actor on a.ActorId equals i
									where a.Role == "Actor"
									select a).ToListAsync();

				if (actors.Count > 0)
				{
					var temp = new List<Movie>();
					foreach (var m in viewModel.Movies)
					{
						if (actors.All(c => m.Actors != null && m.Actors.Contains(c)))
						{
							temp.Add(m);
						}
					}
					viewModel.Movies = temp;
				}
			}

			// director
			if (director != null && director.Count > 0)
			{
				var directors = await (from a in _context.Actors
									   join i in director on a.ActorId equals i
									   where a.Role == "Director"
									   select a).ToListAsync();

				if (directors.Count > 0)
				{
					var temp = new List<Movie>();
					foreach (var m in viewModel.Movies)
					{
						if (directors.All(c => m.Actors != null && m.Actors.Contains(c)))
						{
							temp.Add(m);
						}
					}
					viewModel.Movies = temp;
				}
			}

			// search
			if (!string.IsNullOrWhiteSpace(searchString))
			{
				searchString = searchString.Trim();
				viewModel.Movies = viewModel.Movies
						.Where(m => m.Title.ToLower().Contains(searchString.ToLower())).ToList();
				ViewBag.searchString = searchString;
			}

			// year 
			if (fromYear > 0)
			{
				viewModel.Movies = viewModel.Movies
					.Where(m => m.ReleaseDate != null && m.ReleaseDate.Value.Year >= fromYear)
					.ToList();
				viewModel.FromYear = fromYear;
			}

			if (toYear > 0)
			{
				viewModel.Movies = viewModel.Movies
					.Where(m => m.ReleaseDate != null && m.ReleaseDate.Value.Year <= toYear)
					.ToList();
				viewModel.ToYear = toYear;
			}

			// sort
			switch (sortBy)
			{
				case 1:
					viewModel.Movies = viewModel.Movies.OrderByDescending(m => m.View).ToList();
					break;
				case 2:
					viewModel.Movies = viewModel.Movies.OrderBy(m => m.View).ToList();
					break;
				case 3:
					viewModel.Movies = viewModel.Movies.OrderByDescending(m => m.Rate).ToList();
					break;
				case 4:
					viewModel.Movies = viewModel.Movies.OrderBy(m => m.Rate).ToList();
					break;
				case 5:
					viewModel.Movies = viewModel.Movies.OrderByDescending(m => m.ReleaseDate).ToList();
					break;
				case 6:
					viewModel.Movies = viewModel.Movies.OrderBy(m => m.ReleaseDate).ToList();
					break;
				case 7:
					viewModel.Movies = viewModel.Movies.OrderBy(m => m.Title).ToList();
					break;
				case 8:
					viewModel.Movies = viewModel.Movies.OrderByDescending(m => m.Title).ToList();
					break;
				default:
					viewModel.Movies = viewModel.Movies.OrderByDescending(m => m.View).ToList();
					break;
			}

			ViewBag.View = view;

			// paging
			int totaldata = viewModel.Movies.Count;
			if (pageSize <= 0) pageSize = 10;
			ViewBag.pageSize = pageSize;
			viewModel.PageSize = pageSize;

			int countPages = (int)Math.Ceiling((double)totaldata / pageSize);

			if (currentPage > countPages) currentPage = countPages;
			if (currentPage < 1) currentPage = 1;

			var pagingModel = new PagingModel()
			{
				countpages = countPages,
				currentpage = currentPage,
				generateUrl = (pageNumber) => Url.Action("Index", new
				{
					p = pageNumber,
					pageSize = pageSize,
					searchString = searchString,
					categories = categories,
					actor = actor,
					director = director,
					fromYear = fromYear,
					toYear = toYear,
					sortBy = sortBy,
					view = view,
				})
			};

			ViewBag.pagingModel = pagingModel;
			ViewBag.totaldata = totaldata;

			ViewBag.dataIndex = (currentPage - 1) * pageSize;

			viewModel.Movies = viewModel.Movies.Skip((currentPage - 1) * pageSize)
							 .Take(pageSize)
							 .ToList();

			return View(viewModel);
		}


		[HttpPost]
		public async Task<IActionResult> GetToYear(int fromYear, int toYear)
		{
			var userid = _userManager.GetUserId(this.User);
			ViewBag.ToYear = new SelectList(_context.Movies
							.Where(m => m.ReleaseDate != null
							&& m.Favourites != null
							&& m.Favourites.Where(f => f.UserId == userid).Count() > 0
							&& m.ReleaseDate.Value.Year >= fromYear)
							.Select(m => m.ReleaseDate.Value.Year)
							.Distinct()
							.OrderBy(i => i));

			var viewModel = new ViewCategory();

			if (fromYear > toYear || fromYear == 0)
			{
				toYear = fromYear;
			}

			viewModel.ToYear = toYear;

			return PartialView("_ToYear", viewModel);
		}

		private bool FavouriteExists(int id)
        {
            return _context.Favourite.Any(e => e.FavouriteId == id);
        }
    }
}
