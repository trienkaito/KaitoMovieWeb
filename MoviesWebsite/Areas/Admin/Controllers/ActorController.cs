using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/actor/{action=index}/{id?}")]
    [Authorize(Policy = "Edit")]
    public class ActorController : Controller
    {
        private readonly IActorRepository _actorRepository;
        private readonly ILogger<ActorController> _logger;
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private IWebHostEnvironment _webHostEnvironment;

        public ActorController(IActorRepository actorRepository, ILogger<ActorController> logger, ICloudinaryService cloudinaryService, IWebHostEnvironment webHostEnvironment, AppDbContext context)
        {
            _actorRepository = actorRepository;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword, int? currentPage, int? pageSize)
        {
            var actors = await _actorRepository.GetAllAsync();

            actors = actors.OrderByDescending(a => a.ActorId).ToList();

            if (!string.IsNullOrEmpty(keyword))
            {
                actors = actors.Where(a => (a.Name != null && a.Name.ToLower().Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                            (a.Role != null && a.Role.ToLower().Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
            }

            int totalActors = actors.Count();
            int size = pageSize ?? 10;
            int pageCount = (int)Math.Ceiling(totalActors / (double)size);

            // Điều chỉnh currentPage nếu nó vượt quá pageCount sau khi xóa
            int pageNumber = currentPage ?? 1;
            if (pageNumber > pageCount)
            {
                pageNumber = pageCount;
            }

            var pagedActorList = actors.Skip((pageNumber - 1) * size).Take(size).ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = size;
            ViewBag.PageCount = pageCount;
            ViewBag.Keyword = keyword;

            return View(pagedActorList);
        }

        // GET: Actor/Create
        [HttpGet]
        public IActionResult CreatePartialView()
        {
            var actorVM = new ActorViewModel
            {
                Actor = new Actor(),
                Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Actor", Text = "Actor" },
                    new SelectListItem { Value = "Director", Text = "Director" }
                }
            };
            //Actor actor = new Actor();
            return PartialView("_AddActor", actorVM);
        }

        [HttpGet]
        public async Task<IActionResult> IsActorExists(string name, string role)
        {
            var actorExists = await _context.Actors.AnyAsync(a => a.Name == name && a.Role == role);
            return Json(actorExists);
        }

        [HttpGet]
        public async Task<IActionResult> IsActorUpdateExists(string name, string role, int id)
        {
            var actorExists = await _context.Actors.AnyAsync(a => a.Name == name && a.Role == role && a.ActorId != id);
            return Json(actorExists);
        }

        // POST: Actor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorViewModel actorViewModel, List<IFormFile> files)
        {
            actorViewModel.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Actor", Text = "Actor" },
                    new SelectListItem { Value = "Director", Text = "Director" }
                };

            if (string.IsNullOrWhiteSpace(actorViewModel.Actor.Name))
            {
                TempData["error"] = "Actor created unsuccessfully";
                return RedirectToAction(nameof(Index));
            }

            actorViewModel.Actor.Name = actorViewModel.Actor.Name.Trim();

            if (actorViewModel.Actor.Name.Length < 2)
            {
                TempData["error"] = "Actor created unsuccessfully";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (files != null && files.Any())
                    {
                        var file = files.FirstOrDefault();

                        if (file != null)
                        {
                            var imageUrl = await _cloudinaryService.UploadToCloudinaryAsync(file);
                            if (string.IsNullOrEmpty(imageUrl.Url))
                            {
                                throw new Exception("Upload to Cloudinary failed");
                            }
                            actorViewModel.Actor.Image = imageUrl.Url;
                        }
                    }

                    // Save actor to database
                    await _actorRepository.AddAsync(actorViewModel.Actor);
                    TempData["success"] = "Actor created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating actor");
                }
            }
            else
            {
                TempData["error"] = "Actor created unsuccessfully";
                _logger.LogWarning("ModelState is invalid");
            }
            return RedirectToAction(nameof(Index));
        }


        // GET: Actor/Edit
        [HttpGet]
        public async Task<IActionResult> EditPartialView(int id)
        {
            var actor = await _actorRepository.FindIdAsync(id);

            if (actor == null)
            {
                return NotFound();
            }

            var actorVM = new ActorViewModel
            {
                Actor = actor,
                Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Actor", Text = "Actor" },
                    new SelectListItem { Value = "Director", Text = "Director" }
                },
                imgUrl = actor.Image
            };

            return PartialView("_EditActor", actorVM);
        }

        // POST: Actor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ActorViewModel actorViewModel, List<IFormFile> files)
        {
            actorViewModel.Roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Actor", Text = "Actor" },
                    new SelectListItem { Value = "Director", Text = "Director" }
                };

            if (string.IsNullOrWhiteSpace(actorViewModel.Actor.Name))
            {
                TempData["error"] = "Actor edited unsuccessfully";
                return RedirectToAction(nameof(Index));
            }

            actorViewModel.Actor.Name = actorViewModel.Actor.Name.Trim();

            if (actorViewModel.Actor.Name?.Length < 2)
            {
                TempData["error"] = "Actor edited unsuccessfully";
                return RedirectToAction(nameof(Index));
            }


            if (ModelState.IsValid)
            {
                try
                {
                    if (files != null && files.Any())
                    {
                        var file = files.FirstOrDefault();

                        if (file != null)
                        {
                            var imageUrl = await _cloudinaryService.UploadToCloudinaryAsync(file);
                            if (string.IsNullOrEmpty(imageUrl.Url))
                            {
                                throw new Exception("Upload to Cloudinary failed");
                            }
                            actorViewModel.Actor.Image = imageUrl.Url;
                        }
                    }

                    // Save actor to database
                    await _actorRepository.UpdateAsync(actorViewModel.Actor);
                    TempData["success"] = "Actor edited successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error editing actor");
                }
            }
            else
            {
                TempData["error"] = "Actor edited unsuccessfully";
                _logger.LogWarning("ModelState is invalid");
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Await the asynchronous method to get the actor
            var actor = await _actorRepository.FindIdAsync(id);

            if (actor != null)
            {
                // If the actor is found, delete it
                await _actorRepository.DeleteAsync(actor);
                return Json(new { success = true, message = "Actor deleted successfully" });
            }

            // If the actor is not found, return an error response
            return Json(new { success = false, message = "Error while deleting" });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var actor = await _actorRepository.FindIdAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }
    }
}