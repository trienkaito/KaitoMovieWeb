using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
namespace MoviesWebsite.Controllers
{
    [Route("/comments/{action=index}/{movieId?}")]
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Comments for a specific movie
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Comments.Include(c => c.CommentParent).Include(c => c.Movie).Include(c => c.User);
            return View(await appDbContext.ToListAsync());
        }


        // list of comments for a specific movie follow movieId

        public async Task<IActionResult> ListComments(int movieId)
        {
            var initialComments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(2)
                .ToListAsync();

            return PartialView("_CommentList", initialComments);
        }

        //public async Task<IActionResult> Index(int movieId)
        //{
        //    var initialComments = await _context.Comments
        //        .Include(c => c.User)
        //        .Where(c => c.MovieId == movieId)
        //        .OrderByDescending(c => c.CreatedDate)
        //        .Take(5) // Load first 5 comments
        //        .ToListAsync();

        //    var totalComments = await _context.Comments.CountAsync(c => c.MovieId == movieId);
        //    ViewBag.HasMoreComments = totalComments > 5; // Check if there are more than 5 comments
        //    ViewBag.MovieId = movieId;

        //    return PartialView("_CommentList", initialComments);
        //}

        // Load more comments
        public async Task<IActionResult> LoadMore(int movieId, int skip)
        {
            var moreComments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.CreatedDate)
                .Skip(skip) // Skip the comments that are already displayed
                .Take(2) // Load next 5 comments
                .ToListAsync();

            return PartialView("_CommentList", moreComments);
        }

        // POST: Create a new comment
        [HttpPost]
        [Authorize] // Require user to be authenticated to comment

        public async Task<IActionResult> Create([Bind("CommentId,MovieId,UserId,CommentParentId,CreatedDate,UpdateDate,Content,Hiden")] Comment comment)
        {
            // Validate the model
            if (ModelState.IsValid)
            {
                // Set the user ID to the current user
                // comment.UserId = User.Identity;
                // just get the user id from the claim
                comment.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                comment.CreatedDate = DateTime.Now;
                comment.UpdateDate = DateTime.Now;

                // Add the comment to the database
                _context.Add(comment);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Invalid model state." });
        }

        // public JsonResult Create(int movieId, string content)
        // {
        //     if (string.IsNullOrWhiteSpace(content))
        //     {
        //         return Json(new { success = false, message = "Content cannot be empty." });
        //     }

        //     // Lấy UserId từ claim
        //     var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //     if (userId == null)
        //     {
        //         return Json(new { success = false, message = "User is not authenticated." });
        //     }

        //     var comment = new Comment
        //     {
        //         MovieId = movieId,
        //         UserId = userId,
        //         Content = content,
        //         CreatedDate = DateTime.Now,
        //         UpdateDate = DateTime.Now
        //     };

        //     // Kiểm tra giá trị sau khi khởi tạo
        //     Console.WriteLine($"MovieId: {movieId}, UserId: {userId}, Content: {content}");

        //     try
        //     {
        //         _context.Comments.Add(comment); // Thêm comment vào database
        //         return Json(new { success = true });
        //     }
        //     catch (Exception ex)
        //     {
        //         return Json(new { success = false, message = ex.Message });
        //     }
        // }

        // POST: Edit an existing comment (restricted to the user who created it)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Comment content cannot be empty." });
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.UserId != User.Identity?.Name)
            {
                return Json(new { success = false, message = "Unauthorized action or comment not found." });
            }

            comment.Content = content;
            comment.UpdateDate = DateTime.Now;

            _context.Update(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // DELETE: Delete a comment (restricted to the user who created it)
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null || comment.UserId != User.Identity?.Name)
            {
                return Json(new { success = false, message = "Unauthorized action or comment not found." });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Comment deleted successfully." });
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
