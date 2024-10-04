using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Areas.Admin.Models;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;
using MVC.Models;
using MVC.Utilities;

namespace MoviesWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/Movie/{action=index}/{id?}")]
    [Authorize(Policy = "Edit")]
    public class MovieController : Controller
    {
        [TempData]
        public string Messages { get; set; }

        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public MovieController(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: Admin/Movie
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, string searchString, int pagesize, int? filterId)
        {
            IQueryable<Movie> appDbContext;
            if (filterId != null)
            {
                appDbContext = _context.Movies
                               .Where(m => m.MovieId == filterId)
                               .Include(m => m.Group)
                                .Include(m => m.Categories)
                                .Include(m => m.Comments)
                                .Include(m => m.Episodes)
                                .Include(m => m.Actors)
                                .OrderByDescending(m => m.CreatedDate);
                ViewBag.filterId = filterId;
            }
            else
            {
                appDbContext = _context.Movies
                                .Include(m => m.Group)
                                .Include(m => m.Categories)
                                .Include(m => m.Comments)
                                .Include(m => m.Episodes)
                                .Include(m => m.Actors)
                                .OrderByDescending(m => m.CreatedDate);
            }

            IQueryable<Movie> data;
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                data = appDbContext
                        .Where(m => (m.Title.Contains(searchString))).OrderBy(m => m.Title);
                ViewBag.searchString = searchString;
                if (data.Count() <= 0)
                {
                    TempData["Messages"] = "No data are available.";
                }
            }
            else
            {
                data = appDbContext.OrderByDescending(d => d.MovieId);
            }

            int totaldata = await data.CountAsync();
            if (pagesize <= 0) pagesize = 10;
            ViewBag.pagesize = pagesize;

            int countPages = (int)Math.Ceiling((double)totaldata / pagesize);

            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize,
                    searchString = searchString,
                    filterId = filterId
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totaldata = totaldata;

            ViewBag.dataIndex = (currentPage - 1) * pagesize;

            var dataInPage = await data.Skip((currentPage - 1) * pagesize)
                             .Take(pagesize)
                             .ToListAsync();


            // Debug: Xem dữ liệu từ cơ sở dữ liệu
            var categories = _context.Categories.ToList();
            var directors = _context.Actors.Where(a => a.Role.Equals("Director")).ToList();
            var actors = _context.Actors.Where(a => a.Role.Equals("Actor")).ToList();


            ViewBag.CategoryList = new MultiSelectList(categories, "CategoryId", "Title");
            ViewBag.DirectorList = new MultiSelectList(directors, "ActorId", "Name");
            ViewBag.ActorList = new MultiSelectList(actors, "ActorId", "Name");

            return View(dataInPage);
        }

        // GET: Admin/Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var data = await _context.Movies
                .Include(m => m.Categories)
                .Include(m => m.Actors)
                .Include(m => m.Comments)
                .ThenInclude(c => c.User) // Assuming 'User' is a navigation property in 'Comment'
                .Include(m => m.Episodes)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (data == null)
            {
                return NotFound(); // Return a 404 page if the movie is not found
            }
            data.View += 1;
            await _context.SaveChangesAsync();

            var episode = new EpisodeAdd { MovieId = id };
            ViewData["Episode"] = episode;
            return View(data);
        }

        // GET: Admin/Movie/Create
        public IActionResult Create()
        {
            // Trả về Partial View
            return PartialView("_AddMovie");
        }

        [HttpGet]
        public async Task<IActionResult> CheckMovieExists(string title)
        {
            string slug = AppUtilities.GenerateSlug(title);
            var movieExists = await _context.Movies.AnyAsync(m => m.Title == title || m.Slug == slug);
            return Json(movieExists);
        }

        [HttpGet]
        public async Task<IActionResult> CheckMovieUpdateExists(string title, int id)
        {
            string slug = AppUtilities.GenerateSlug(title);
            var movieExists = await _context.Movies.AnyAsync(m => (m.Title == title || m.Slug == slug) && m.MovieId != id);
            return Json(movieExists);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] MovieCreate movie)
        {
            if (ModelState.IsValid)
            {

                movie.CreatedDate = DateTime.Now;
                movie.UpdateDate = DateTime.Now;
                movie.Slug = AppUtilities.GenerateSlug(movie.Title);

                //var duplicationSlug = _context.Movies.Any(c => c.Slug == movie.Slug);
                //var duplicationTitle = _context.Movies.Any(c => c.Title == movie.Title);
                // var duplicationUrl = _context.Episodes.Any(e => e.Link == movie.Url);


                //if (duplicationTitle)
                //{
                //    ModelState.AddModelError("", "Title has already existed.");
                //    Messages = "Title has already existed.";
                //    return RedirectToAction(nameof(Index));
                //}
                //if (duplicationSlug)
                //{
                //    ModelState.AddModelError("", "Slug has already existed.");
                //    Messages = "Slug has already existed.";
                //    return RedirectToAction(nameof(Index));
                //}

                // if (duplicationUrl)
                // {
                //     ModelState.AddModelError("", "URL has already existed.");
                //     Messages = "URL has already existed.";
                //     return RedirectToAction(nameof(Index));
                // }


                movie.ImageMovies = new();
                if (movie.FileUploads != null && movie.FileUploads.Count > 0)
                {
                    foreach (var file in movie.FileUploads)
                    {
                        var imageUrl = await _cloudinaryService.UploadToCloudinaryAsync(file);
                        if (string.IsNullOrEmpty(imageUrl.Url))
                        {
                            Messages = "Cann't upload image!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            movie.ImageMovies.Add(new ImageMovie { Url = imageUrl.Url });
                        }
                    }
                }

                if (movie.ImageMovies.Any())
                {
                    movie.Image = movie.ImageMovies.First().Url;
                }

                if (movie.TrailerUpload != null)
                {
                    var trailerUrl = await _cloudinaryService.UploadToCloudinaryAsync(movie.TrailerUpload);
                    if (string.IsNullOrEmpty(trailerUrl.Url))
                    {
                        Messages = "Cann't upload trailer!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        movie.Trailer = trailerUrl.Url;
                    }
                }


                // add CategoryMovie
                movie.Categories = new();
                if (movie.CategoriesIds != null && movie.CategoriesIds.Length > 0)
                {
                    foreach (var cateId in movie.CategoriesIds)
                    {
                        var cate = _context.Categories.FirstOrDefault(c => c.CategoryId == cateId);
                        if (cate != null)
                        {
                            movie.Categories.Add(cate);
                        }
                    }
                }
                // Add ActorMovie
                movie.Actors = new();
                if (movie.DirectorIds != null && movie.DirectorIds.Length > 0)
                {
                    foreach (var actorId in movie.DirectorIds)
                    {
                        var actor = _context.Actors.FirstOrDefault(a => a.ActorId == actorId);
                        if (actor != null)
                        {
                            movie.Actors.Add(actor);
                        }
                    }
                }
                if (movie.ActorIds != null && movie.ActorIds.Length > 0)
                {
                    foreach (var actorId in movie.ActorIds)
                    {
                        var actor = _context.Actors.FirstOrDefault(a => a.ActorId == actorId);
                        if (actor != null)
                        {
                            movie.Actors.Add(actor);
                        }
                    }
                }


                _context.Add(movie);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            Messages = "Can't add movie";

            var categories = _context.Categories.ToList();
            var directors = _context.Actors.Where(a => a.Role.Equals("Director")).ToList();
            var actors = _context.Actors.Where(a => a.Role.Equals("Actor")).ToList();


            ViewBag.CategoryList = new MultiSelectList(categories, "CategoryId", "Title");
            ViewBag.DirectorList = new MultiSelectList(directors, "ActorId", "Name");
            ViewBag.ActorList = new MultiSelectList(actors, "ActorId", "Name");

            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/Movie/Edit/5
        public async Task<IActionResult> EditPartialView(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var movie = await _context.Movies
                               .Where(m => m.MovieId == id)
                               .Include(m => m.Group)
                                .Include(m => m.Categories)
                                .Include(m => m.Comments)
                                .Include(m => m.Episodes)
                                .Include(m => m.Actors).FirstOrDefaultAsync();
            if (movie == null)
            {
                return NotFound();
            }

            var editMovie = new MovieEdit()
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                Description = movie.Description,
                Image = movie.Image,
                Series = movie.Series,
                CategoriesIds = movie.Categories?.Select(m => m.CategoryId).ToArray(),
                DirectorIds = movie.Actors?.Where(m => m.Role == "Director").Select(m => m.ActorId).ToArray(),
                ActorIds = movie.Actors?.Where(m => m.Role == "Actor").Select(m => m.ActorId).ToArray(),
                Trailer = movie.Trailer,
                ReleaseDate = movie.ReleaseDate,
            };



            var categories = _context.Categories.ToList();
            var directors = _context.Actors.Where(a => a.Role.Equals("Director")).ToList();
            var actors = _context.Actors.Where(a => a.Role.Equals("Actor")).ToList();


            ViewBag.CategoryList = new MultiSelectList(categories, "CategoryId", "Title");
            ViewBag.DirectorList = new MultiSelectList(directors, "ActorId", "Name");
            ViewBag.ActorList = new MultiSelectList(actors, "ActorId", "Name");

            return PartialView("_EditMovie", editMovie);
        }

        // POST: Admin/Movie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] MovieEdit movie)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMovie = await _context.Movies
                        .Include(m => m.Categories)
                        .Include(m => m.Actors)
                        .Include(m => m.Episodes)
                        .FirstOrDefaultAsync(m => m.MovieId == id);

                    if (existingMovie == null)
                    {
                        return NotFound();
                    }

                    //var duplicationSlug = _context.Movies.Any(c => c.Slug == movie.Slug && c.MovieId != id);
                    //var duplicationTitle = _context.Movies.Any(c => c.Title == movie.Title && c.MovieId != id);


                    //if (duplicationTitle)
                    //{
                    //    ModelState.AddModelError("", "Title has already existed.");
                    //    Messages = "Title has already existed.";
                    //    return RedirectToAction(nameof(Index));
                    //}
                    //if (duplicationSlug)
                    //{
                    //    ModelState.AddModelError("", "Slug has already existed.");
                    //    Messages = "Slug has already existed.";
                    //    return RedirectToAction(nameof(Index));
                    //}


                    List<string> imageUrls = new();

                    if (movie.FileUploads != null && movie.FileUploads.Count > 0)
                    {

                        foreach (var file in movie.FileUploads)
                        {
                            var imageUrl = await _cloudinaryService.UploadToCloudinaryAsync(file);
                            if (string.IsNullOrEmpty(imageUrl.Url))
                            {
                                Messages = "Cann't upload image!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                imageUrls.Add(imageUrl.Url);
                            }
                        }

                        // Update existingMovie images
                        movie.Image = string.Join(",", imageUrls);
                    }

                    // If no new image, keep old image
                    if (imageUrls.Any())
                    {
                        movie.Image = imageUrls.First();
                    }

                    //if (movie.FileUpload == null)
                    //{
                    //    ModelState.AddModelError("", "Please select an image");
                    //}
                    //if (movie.FileUpload != null)
                    //{
                    //    var imageUrl = await _cloudinaryService.UploadToCloudinaryAsync(movie.FileUpload);
                    //    if (string.IsNullOrEmpty(imageUrl.Url))
                    //    {
                    //        Messages = "Can't upload image!";
                    //        return RedirectToAction(nameof(Index));
                    //    }

                    //    movie.Image = imageUrl.Url;
                    //}

                    // Update the movie details
                    existingMovie.Title = movie.Title;
                    existingMovie.Description = movie.Description;
                    existingMovie.UpdateDate = DateTime.Now;
                    existingMovie.ReleaseDate = movie.ReleaseDate;
                    existingMovie.Trailer = movie.Trailer;
                    existingMovie.Series = movie.Series;
                    existingMovie.Slug = AppUtilities.GenerateSlug(movie.Title);


                    if (movie.FileUploads != null && movie.FileUploads.Count > 0)
                    {

                        foreach (var file in movie.FileUploads)
                        {
                            var imageUrl = await _cloudinaryService.UploadToCloudinaryAsync(file);
                            if (string.IsNullOrEmpty(imageUrl.Url))
                            {
                                Messages = "Cann't upload image!";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                existingMovie.Image = string.Join(",", imageUrls);
                            }
                        }
                    }
                    else
                    {
                        // Nếu không có ảnh mới, giữ nguyên ảnh cũ
                        existingMovie.Image = existingMovie.Image;
                    }

                    // Update categories
                    existingMovie.Categories.Clear();
                    if (movie.CategoriesIds != null && movie.CategoriesIds.Length > 0)
                    {
                        foreach (var cateId in movie.CategoriesIds)
                        {
                            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == cateId);
                            if (category != null)
                            {
                                existingMovie.Categories.Add(category);
                            }
                        }
                    }

                    // Update actors
                    existingMovie.Actors.Clear();
                    if (movie.DirectorIds != null && movie.DirectorIds.Length > 0)
                    {
                        foreach (var actorId in movie.DirectorIds)
                        {
                            var actor = await _context.Actors.FirstOrDefaultAsync(a => a.ActorId == actorId);
                            if (actor != null)
                            {
                                existingMovie.Actors.Add(actor);
                            }
                        }
                    }
                    if (movie.ActorIds != null && movie.ActorIds.Length > 0)
                    {
                        foreach (var actorId in movie.ActorIds)
                        {
                            var actor = await _context.Actors.FirstOrDefaultAsync(a => a.ActorId == actorId);
                            if (actor != null)
                            {
                                existingMovie.Actors.Add(actor);
                            }
                        }
                    }

                    if (movie.TrailerUpload != null)
                    {
                        var trailerUrl = await _cloudinaryService.UploadToCloudinaryAsync(movie.TrailerUpload);
                        if (string.IsNullOrEmpty(trailerUrl.Url))
                        {
                            Messages = "Cann't upload trailer!";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            existingMovie.Trailer = trailerUrl.Url;
                        }
                    }


                    _context.Update(existingMovie);
                    await _context.SaveChangesAsync();
                    var nullEpisodesToDelete = _context.Episodes.Where(m => m.MovieId == null);
                    _context.Episodes.RemoveRange(nullEpisodesToDelete);
                    await _context.SaveChangesAsync();

                    Messages = "Movie updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw new DbUpdateConcurrencyException();
                    }
                }
            }

            Messages = "Failed to update movie.";

            var categories = _context.Categories.ToList();
            var directors = _context.Actors.Where(a => a.Role.Equals("Director")).ToList();
            var actors = _context.Actors.Where(a => a.Role.Equals("Actor")).ToList();

            ViewBag.CategoryList = new MultiSelectList(categories, "CategoryId", "Title");
            ViewBag.DirectorList = new MultiSelectList(directors, "ActorId", "Name");
            ViewBag.ActorList = new MultiSelectList(actors, "ActorId", "Name");

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Movie/Delete/5
        public async Task<IActionResult> DeletePartialView(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteMovie", movie);
        }

        // POST: Admin/Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.Include(m => m.Episodes).FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                var episodesToDelete = _context.Episodes.Where(m => m.MovieId == id);
                _context.Episodes.RemoveRange(episodesToDelete);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEpisode(EpisodeAdd episode)
        {
            if (!ModelState.IsValid)
            {
                return await ReturnMovieDetailsWithModal(episode.MovieId);
            }
            if (episode.Title.Trim() == null || episode.Link == null)
            {
                ModelState.AddModelError("Title", "Title must required");
                return await ReturnMovieDetailsWithModal(episode.MovieId);
            }
            var movie = await _context.Movies
                .Include(m => m.Episodes)
                .FirstOrDefaultAsync(m => m.MovieId == episode.MovieId);

            if (movie == null)
            {
                return NotFound();
            }

            if (movie.Episodes.Any(e => e.Title.Equals(movie.Title + " " + episode.Title, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Title", "Title đã tồn tại");
                return await ReturnMovieDetailsWithModal(episode.MovieId);
            }

            // var newEpisode = new Episode
            // {

            //     Title = movie.Title + " " + episode.Title,
            //     Slug = AppUtilities.GenerateSlug(movie.Title + " " + episode.Title),
            //     Link = episode.Link,
            //     MovieId = episode.MovieId
            // };

            var linkVideo = await _cloudinaryService.UploadToCloudinaryAsync(episode.Link);
            var newEpisode = new Episode
            {
                Title = movie.Title + " " + episode.Title,
                Slug = AppUtilities.GenerateSlug(movie.Title + " " + episode.Title),
                Link = linkVideo.Url,
                MovieId = episode.MovieId
            };

            movie.Episodes.Add(newEpisode);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = episode.MovieId });
        }

        private async Task<IActionResult> ReturnMovieDetailsWithModal(int? movieId)
        {
            var movieWithDetails = await _context.Movies
                .Include(m => m.Categories)
                .Include(m => m.Actors)
                .Include(m => m.Comments).ThenInclude(c => c.User)
                .Include(m => m.Episodes)
                .FirstOrDefaultAsync(m => m.MovieId == movieId);

            if (movieWithDetails == null)
            {
                return NotFound();
            }

            ViewBag.modal = "show";
            return View("Details", movieWithDetails);
        }
        [HttpGet]
        public IActionResult EditEpisode(int id)
        {
            var episode = _context.Episodes.Include(e => e.Movie).FirstOrDefault(e => e.EpisodeId == id);
            if (episode == null || episode.Movie == null)
            {
                return NotFound();
            }

            var movie = episode.Movie;

            if (movie == null)
            {
                return NotFound();
            }




            var model = new EpisodeAdd
            {
                EpisodeId = episode.EpisodeId,
                Title = movie.Title + " " + episode.Title,
                Slug = AppUtilities.GenerateSlug(movie.Title + " " + episode.Title),
                // Link = episode.Link;
                MovieId = episode.MovieId
            };

            return PartialView("_EditEpisode", model);
        }
        public JsonResult IsFilterExist(string Title, int EpisodeId)
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return Json(false);
            }
            var episodes = _context.Episodes.Include(e => e.Movie).FirstOrDefault(e => e.EpisodeId == EpisodeId);
            var movie = episodes.Movie;
            var filterExist = _context.Episodes.Any(t => t.Title == (movie.Title + " " + Title.Trim()) && t.EpisodeId != episodes.EpisodeId);
            return Json(filterExist);
        }
        [HttpPost]
        public async Task<IActionResult> EditEpisode(EpisodeAdd model)
        {
            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, string>();

                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    errors["TitleError"] = "Title is required";
                }
                // if (string.IsNullOrWhiteSpace(model.Link))
                // {
                //     errors["LinkError"] = "Link is required";
                // }
                if (model.Link == null)
                {
                    errors["LinkError"] = "Link is required";
                }

                return Json(new { success = false, errors });
            }
            // if (ModelState.IsValid)
            // {
            //     // Kiểm tra nếu có file được upload
            //     if (model.Link != null && model.Link.Length > 0)
            //     {
            //         // Lưu file vào thư mục mong muốn (ví dụ "wwwroot/uploads")

            //     }

            //     // Xử lý lưu thông tin khác vào cơ sở dữ liệu
            // }

            // if(model.Link != null){
            //     var linkVideo = await _cloudinaryService.UploadToCloudinaryAsync(model.Link);
            //     model.Link = linkVideo.Url;
            // }


            var episode = await _context.Episodes.Include(e => e.Movie).FirstOrDefaultAsync(e => e.EpisodeId == model.EpisodeId);

            var movie = episode.Movie;

            if (movie == null)
            {
                return NotFound();
            }

            if (episode == null)
            {
                return NotFound();
            }
            var linkVideo = await _cloudinaryService.UploadToCloudinaryAsync(model.Link);
            episode.Link = linkVideo.Url;
            // var linkVideo = await _cloudinaryService.UploadToCloudinaryAsync(model.Link);

            episode.Title = movie.Title + " " + model.Title;
            episode.Slug = AppUtilities.GenerateSlug(movie.Title + " " + model.Title);
            // episode.Link = linkVideo.Url;
            // episode.Link = model.Link;

            _context.Episodes.Update(episode);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEpisode(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null)
            {
                return Json(new { success = false, message = "Episode not found" });
            }

            _context.Episodes.Remove(episode);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
