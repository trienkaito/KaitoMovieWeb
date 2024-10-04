using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;

namespace MoviesWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Route("/admin/comments/{action=index}/{id?}")]
    [Authorize(Policy = "Edit")]
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Comments
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Comments.Include(c => c.CommentParent).Include(c => c.Movie).Include(c => c.User);
            return View(await appDbContext.ToListAsync());
        }


        //public async Task<IActionResult> Index(int movieId)
        //{
        //    var initialComments = await _context.Comments
        //        .Where(x => x.MovieId == movieId)
        //        .OrderByDescending(o => o.CreatedDate)
        //        .Take(2)
        //        .ToListAsync();

        //    var totalComment = await _context.Comments.CountAsync(m => m.MovieId == movieId);
        //    ViewBag.HasMoreComments = totalComment > 2;
        //    ViewBag.MovieId = movieId;
        //    return PartialView(initialComments);

        //}
        public async Task<IActionResult> LoadMore(int movieId, int skip)
        {
            var moreComments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.CreatedDate)
                .Skip(skip)
                .Take(2)
                .ToListAsync();

            return PartialView("_CommentList", moreComments);
        }

        // GET: Admin/Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.CommentParent)
                .Include(c => c.Movie)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Admin/Comments/Create
        public IActionResult Create()
        {
            ViewData["CommentParentId"] = new SelectList(_context.Comments, "CommentId", "Content");
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Slug");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            //return View();
            return PartialView("_CreateComment");
        }

        // POST: Admin/Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,MovieId,UserId,CommentParentId,CreatedDate,UpdateDate,Content,Hiden")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CreatedDate = DateTime.Now;
                comment.UpdateDate = DateTime.Now;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Movie", new { id = comment.MovieId, area = "Admin" });
            }
            ViewData["CommentParentId"] = new SelectList(_context.Comments, "CommentId", "Content", comment.CommentParentId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Slug", comment.MovieId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", comment.UserId);
            return RedirectToAction("Details", "Movie", new { id = comment.MovieId, area = "Admin" });
        }

        // GET: Admin/Comments/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var comment = await _context.Comments.FindAsync(id);
        //    if (comment == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["CommentParentId"] = new SelectList(_context.Comments, "CommentId", "Content", comment.CommentParentId);
        //    ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Slug", comment.MovieId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", comment.UserId);
        //    return View(comment);
        //}

        // POST: Admin/Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("CommentId,MovieId,UserId,CommentParentId,CreatedDate,UpdateDate,Content,Hiden")] Comment comment)
        //{
        //    if (id != comment.CommentId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(comment);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CommentExists(comment.CommentId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CommentParentId"] = new SelectList(_context.Comments, "CommentId", "Content", comment.CommentParentId);
        //    ViewData["MovieId"] = new SelectList(_context.Movies, "MovieId", "Slug", comment.MovieId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", comment.UserId);
        //    return View(comment);
        //}
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,Content")] Comment comment)
        {
            if (id != comment.CommentId)
            {
                return Json(new { success = false, message = "Comment ID mismatch." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingComment = await _context.Comments.FindAsync(id);
                    if (existingComment == null)
                    {
                        return Json(new { success = false, message = "Comment not found." });
                    }

                    // Update only the fields that are being edited
                    existingComment.Content = comment.Content;
                    existingComment.UpdateDate = DateTime.Now;

                    _context.Update(existingComment);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }

            // If we reach here, something went wrong with the model validation
            return Json(new { success = false, message = "Invalid model state." });
        }

        // Helper method to check if the comment exists



        // GET: Admin/Comments/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var comment = await _context.Comments
        //        .Include(c => c.CommentParent)
        //        .Include(c => c.Movie)
        //        .Include(c => c.User)
        //        .FirstOrDefaultAsync(m => m.CommentId == id);
        //    if (comment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(comment);
        //}

        // POST: Admin/Comments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Actor deleted successfully" });

            }
            return Json(new { success = false, message = "Error while deleting" });

            //return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
