using Microsoft.AspNetCore.Mvc;
using MoviesWebsite.Areas.Admin.Controllers;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Controllers
{
	public class HomeActorController : Controller
	{
		private readonly IActorRepository _actorRepository;
		private readonly ILogger<ActorController> _logger;
		private readonly AppDbContext _context;

		public HomeActorController(IActorRepository actorRepository, ILogger<ActorController> logger, AppDbContext context)
		{
			_actorRepository = actorRepository;
			_logger = logger;
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> Index(int? currentPage, int? pageSize, string searchName, string searchRole, string sortOrder)
		{
			var actors = await _actorRepository.GetAllAsync();

			if (!string.IsNullOrEmpty(searchName))
			{
				actors = actors.Where(a => a.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
			}

			if (!string.IsNullOrEmpty(searchRole))
			{
				actors = actors.Where(a => a.Role.Contains(searchRole, StringComparison.OrdinalIgnoreCase)).ToList();
			}

			//actors = actors.OrderByDescending(a => a.ActorId).ToList();

			switch (sortOrder)
			{
				case "name_asc":
					actors = actors.OrderBy(i => i.Name).ToList();
					break;
				case "name_desc":
					actors = actors.OrderByDescending(i => i.Name).ToList();
					break;
				default:
					actors = actors.OrderBy(i => i.Name).ToList(); // Mặc định sắp xếp theo tên tăng dần
					break;
			}

			int totalActors = actors.Count();
			int size = pageSize ?? 12;
			int pageCount = (int)Math.Ceiling(totalActors / (double)size);
			int pageNumber = currentPage ?? 1;
			var pagedActorList = actors.Skip((pageNumber - 1) * size).Take(size).ToList();

			ViewBag.CurrentPage = pageNumber;
			ViewBag.PageSize = size;
			ViewBag.PageCount = pageCount;
			ViewBag.SearchName = searchName;
			ViewBag.SearchRole = searchRole;
			ViewBag.TotalActors = totalActors;

			ViewBag.SortOrder = sortOrder;

			return View(pagedActorList);
		}

		[HttpGet]
		public async Task<IActionResult> IndexList(int? currentPageList, int? pageSizeList, string searchNameList, string searchRoleList, string sortOrderList)
		{
			var actors = await _actorRepository.GetAllAsync();

			if (!string.IsNullOrEmpty(searchNameList))
			{
				actors = actors.Where(a => a.Name.Contains(searchNameList, StringComparison.OrdinalIgnoreCase)).ToList();
			}

			if (!string.IsNullOrEmpty(searchRoleList))
			{
				actors = actors.Where(a => a.Role.Contains(searchRoleList, StringComparison.OrdinalIgnoreCase)).ToList();
			}

			switch (sortOrderList)
			{
				case "name_asc":
					actors = actors.OrderBy(i => i.Name).ToList();
					break;
				case "name_desc":
					actors = actors.OrderByDescending(i => i.Name).ToList();
					break;
				default:
					actors = actors.OrderBy(i => i.Name).ToList(); // Mặc định sắp xếp theo tên tăng dần
					break;
			}


			//actors = actors.OrderByDescending(a => a.ActorId).ToList();

			int totalActorsList = actors.Count();
			int sizeList = pageSizeList ?? 12;
			int pageCountList = (int)Math.Ceiling(totalActorsList / (double)sizeList);

			int pageNumberList = currentPageList ?? 1;
			var pagedActorList = actors.Skip((pageNumberList - 1) * sizeList).Take(sizeList).ToList();

			ViewBag.CurrentPageList = pageNumberList;
			ViewBag.PageSizeList = sizeList;
			ViewBag.PageCountList = pageCountList;
			ViewBag.SearchNameList = searchNameList;
			ViewBag.SearchRoleList = searchRoleList;
			ViewBag.TotalActorsList = totalActorsList;

			ViewBag.SortOrderList = sortOrderList;

			return View(pagedActorList);
		}


		[HttpGet]
		public async Task<IActionResult> Details(int id, int? currentPage, int? pageSize, string activeTab = "overview")
		{
			var actor = await _actorRepository.FindActorIdAsync(id);
			if (actor == null)
			{
				return NotFound();
			}

			// Paginate movies
			var movies = actor.Movies ?? new List<Movie>();
			int totalMovies = movies.Count();
			int size = pageSize ?? 10;
			int pageNumber = currentPage ?? 1;
			int pageCount = (int)Math.Ceiling(totalMovies / (double)size);

			var pagedMovies = movies.Skip((pageNumber - 1) * size).Take(size).ToList();

			var latestMovies = movies
				 .OrderByDescending(m => m.ReleaseDate)  // Assuming ReleaseDate is used to determine latest movies
				 .Take(3)
				 .ToList();

			ViewBag.CurrentPage = pageNumber;
			ViewBag.PageSize = pageSize;
			ViewBag.PageCount = pageCount;
			ViewBag.TotalMovies = totalMovies;
			ViewBag.ActiveTab = activeTab;  // Pass the activeTab to the view
			ViewBag.ActorId = id; // Make sure to set ActorId

			// Pass paginated movies and actor data to the view
			var viewModel = new ActorDetailsViewModel
			{
				Actor = actor,
				Movies = pagedMovies,
				LatestMovies = latestMovies // Add LatestMovies to the ViewModel
			};

			return View(viewModel);
		}

	}
}
