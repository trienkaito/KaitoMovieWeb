using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using MVC.Models;
using Org.BouncyCastle.Tsp;
using static MoviesWebsite.Controllers.ViewMoviesController;

namespace MoviesWebsite.Controllers
{
	[Authorize]
	[Route("/history/{action=Index}")]
	public class HistoriesController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _userManager;

		public HistoriesController(AppDbContext context, UserManager<AppUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public class FilterBy
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class ViewHistory
		{
			public List<History>  Histories { get; set; }
			public int TotalMovies { get; set; }

			[Display(Name = "Filter By:")]
			public int FilterBy { get; set; }

			[Display(Name = "Movies per page:")]
			public int PageSize { get; set; }
		}

		// GET: Histories
		public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage,
							int pageSize,
							int filterBy)
		{
			var viewModel = new ViewHistory();
			var userid = _userManager.GetUserId(this.User);

			var filterBys = new List<FilterBy>()
			{
				new FilterBy() {Id = 1, Name = "All"},
				new FilterBy() {Id = 2, Name = "Today"},
				new FilterBy() {Id = 3, Name = "Yesterday"},
				new FilterBy() {Id = 4, Name = "In 7 Days"},
				new FilterBy() {Id = 5, Name = "In 30 Days"},
				new FilterBy() {Id = 6, Name = "In 365 Days"},


			};
			ViewBag.FilterBy = new SelectList(filterBys, nameof(FilterBy.Id), nameof(FilterBy.Name));

			var moviesPerPages = new List<MoviesPerPage>()
			{
				new MoviesPerPage() {PageSize=10, Name = "10 Movies"},
				new MoviesPerPage() {PageSize=20, Name = "20 Movies"}
			};
			ViewBag.MoviesPerPages = new SelectList(moviesPerPages, nameof(MoviesPerPage.PageSize), nameof(MoviesPerPage.Name));


			// filter
			viewModel.Histories = _context.Histories.Include(h => h.Episode).ThenInclude(e => e.Movie)
					.Where(h => h.UserId == userid)
					.ToList();

			switch (filterBy)
			{
				case 1:
					viewModel.Histories = viewModel.Histories.OrderByDescending(h => h.Time).ToList();
					break;
				case 2:
					viewModel.Histories = viewModel.Histories.Where(h => h.Time != null && h.Time?.Date == DateTime.Now.Date)
						.OrderByDescending(h => h.Time).ToList();
					break;
				case 3:
					viewModel.Histories = viewModel.Histories.Where(h => h.Time != null && h.Time?.Date == DateTime.Now.AddDays(-1).Date)
						.OrderByDescending(h => h.Time).ToList();
					break;
				case 4:
					viewModel.Histories = viewModel.Histories.Where(h => h.Time != null && h.Time?.Date >= DateTime.Now.AddDays(-7).Date)
						.OrderByDescending(h => h.Time).ToList();
					break;
				case 5:
					viewModel.Histories = viewModel.Histories.Where(h => h.Time != null && h.Time?.Date >= DateTime.Now.AddDays(-30).Date)
						.OrderByDescending(h => h.Time).ToList();
					break;
				case 6:
					viewModel.Histories = viewModel.Histories.Where(h => h.Time != null && h.Time?.Date >= DateTime.Now.AddDays(-365).Date)
						.OrderByDescending(h => h.Time).ToList();
					break;
				default:
					viewModel.Histories = viewModel.Histories.OrderByDescending(h => h.Time).ToList();
					break;
			}

			// paging
			int totaldata = viewModel.Histories.Count;
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
					filterBy = filterBy,
				})
			};

			ViewBag.pagingModel = pagingModel;
			ViewBag.totaldata = totaldata;

			ViewBag.dataIndex = (currentPage - 1) * pageSize;

			viewModel.Histories = viewModel.Histories.Skip((currentPage - 1) * pageSize)
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


		private bool HistoryExists(int id)
		{
			return _context.Histories.Any(e => e.HistoryId == id);
		}
	}
}
