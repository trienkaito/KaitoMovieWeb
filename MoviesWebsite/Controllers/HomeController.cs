using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MoviesWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Detail(string? slug, int? currentPage, int? pageSize, string sortOrder, string activeTab = "overview")
        {
            var movie = await _context.Movies
                .Include(m => m.Categories)
                .Include(m => m.Actors)
                .Include(m => m.Comments)
                .Include(m => m.Evaluates)
                .ThenInclude(c => c.User)
                .Include(m => m.Episodes)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (movie == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isFavorited = false;

            if (!string.IsNullOrEmpty(userId))
            {
                isFavorited = await _context.Favourite
                    .AnyAsync(f => f.MovieId == movie.MovieId && f.UserId == userId);
            }

            double averageRating = await CalculateAverageRating(movie.MovieId);
            //ViewBag.AverageRating = averageRating;
            movie.Rate = (float)averageRating;


            ViewBag.IsFavorited = isFavorited;

            if (activeTab == "overview")
            {
                var topReviews = await _context.Evaluates
                    .Include(e => e.User)
                    .Where(e => e.MovieId == movie.MovieId)
                    .OrderByDescending(e => e.Star)
                    .ThenByDescending(e => e.CreatedDate)
                    .Take(3)
                    .ToListAsync();

                ViewBag.TopReviews = topReviews;
            }

            // Paginated reviews
            var reviews = movie.Evaluates.AsQueryable();

            // Apply sorting based on the selected sortOrder
            switch (sortOrder)
            {
                case "rating_asc":
                    reviews = reviews.OrderBy(r => r.Star);
                    break;
                case "rating_desc":
                    reviews = reviews.OrderByDescending(r => r.Star);
                    break;
                case "date_asc":
                    reviews = reviews.OrderBy(r => r.CreatedDate);
                    break;
                case "date_desc":
                    reviews = reviews.OrderByDescending(r => r.CreatedDate);
                    break;
                default:
                    reviews = reviews.OrderByDescending(r => r.CreatedDate);
                    break;
            }

            int totalReviews = reviews.Count();
            int size = pageSize ?? 10; // Default to 10 reviews per page if not specified
            int pageNumber = currentPage ?? 1;
            int pageCount = (int)Math.Ceiling(totalReviews / (double)size);

            var pagedReviews = reviews
                .Skip((pageNumber - 1) * size)
                .Take(size)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = size;
            ViewBag.PageCount = pageCount;
            ViewBag.TotalReviews = totalReviews;
            ViewBag.Reviews = pagedReviews;
            ViewBag.ActiveTab = activeTab;
            ViewBag.Slug = slug;
            ViewBag.SortOrder = sortOrder;

            var viewModel = new MovieDetaisViewModel
            {
                Movie = movie,
                Reviews = pagedReviews
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            // Kiểm tra nếu người dùng đã đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy user ID từ Claims
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện hành động này." });
            }

            // Kiểm tra nếu phim đã tồn tại trong danh sách yêu thích của người dùng
            var existingFavorite = await _context.Favourite
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (existingFavorite != null)
            {
                // Nếu phim đã có trong danh sách yêu thích, xóa khỏi danh sách
                _context.Favourite.Remove(existingFavorite);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa khỏi danh sách yêu thích.", isFavorited = false });
            }

            // Thêm phim vào danh sách yêu thích nếu chưa có
            var favourite = new Favourite
            {
                MovieId = movieId,
                UserId = userId,
                CreatedDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            _context.Favourite.Add(favourite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào danh sách yêu thích.", isFavorited = true });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int movieId, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (rating < 1 || rating > 10)
            {
                return BadRequest("Rating must be between 1 and 10.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return BadRequest("Comment cannot be empty.");
            }

            // Check if comment contains link
            var urlPattern = @"(http[s]?://|www\.)\S+"; // Regular expression to detect URL
            if (Regex.IsMatch(comment, urlPattern))
            {
                return BadRequest("Review cannot contain links.");
            }

            var movie = await _context.Movies
                .Include(m => m.Categories)
                .Include(m => m.Actors)
                .Include(m => m.Comments)
                .Include(m => m.Evaluates)
                .ThenInclude(c => c.User)
                .Include(m => m.Episodes)
                .FirstOrDefaultAsync(m => m.MovieId == movieId);

            if (movie == null)
            {
                return NotFound();
            }

            var existingEvaluate = await _context.Evaluates
                .FirstOrDefaultAsync(e => e.MovieId == movieId && e.UserId == userId);

            if (existingEvaluate != null)
            {
                existingEvaluate.Star = rating;
                existingEvaluate.Content = comment;
                existingEvaluate.CreatedDate = DateTime.Now;
                _context.Evaluates.Update(existingEvaluate);
            }
            else
            {
                var evaluate = new Evaluate
                {
                    MovieId = movieId,
                    UserId = userId,
                    Star = rating,
                    Content = comment,
                    CreatedDate = DateTime.Now,
                };
                _context.Evaluates.Add(evaluate);
            }

            await _context.SaveChangesAsync();

            // Calculate the new average rating
            double averageRating = await CalculateAverageRating(movie.MovieId);
            //ViewBag.AverageRating = averageRating; // Cập nhật ViewBag

            movie.Rate = (float)averageRating;
            await _context.SaveChangesAsync();

            // Lấy danh sách đánh giá mới nhất
            var reviews = await _context.Evaluates
                .Include(e => e.User)
                .Where(e => e.MovieId == movieId)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return PartialView("_ReviewsPartial", reviews);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int evaluateId, int movieId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var evaluate = await _context.Evaluates.Include(e => e.Movie)
                .FirstOrDefaultAsync(e => e.EvaluateId == evaluateId && e.UserId == userId);

            var movie = evaluate.Movie;

            if (evaluate == null)
            {
                return NotFound();
            }

            _context.Evaluates.Remove(evaluate);
            await _context.SaveChangesAsync();

            // Calculate the new average rating
            double averageRating = await CalculateAverageRating(movieId);
            //ViewBag.AverageRating = averageRating; // Cập nhật ViewBag
            movie.Rate = (float)averageRating;
            await _context.SaveChangesAsync();

            // Lấy danh sách đánh giá mới nhất
            var reviews = await _context.Evaluates
              .Include(e => e.User) // Tải kèm thông tin User
              .Where(e => e.MovieId == movieId)
              .OrderByDescending(e => e.CreatedDate)
              .ToListAsync();


            return PartialView("_ReviewsPartial", reviews);
        }

        public async Task<double> CalculateAverageRating(int movieId)
        {
            var movieEvaluates = await _context.Evaluates
                .Where(e => e.MovieId == movieId)
                .ToListAsync();

            if (movieEvaluates.Count == 0)
            {
                return 0;
            }

            // Calculate the average rating
            var averageRating = Math.Round(movieEvaluates.Average(e => e.Star), 2);
            return averageRating;
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
