using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MVC.Models;
using MVC.Utilities;

namespace MoviesWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("/admin/categories/{action=index}/{id?}")]
    [Authorize(Policy = "Edit")]
    public class CategoriesController : Controller
    {
        [TempData]
        public string Messages { get; set; }
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, string searchString, int pagesize, int? filterId)
        {
            IQueryable<Category> appDbContext;
            if (filterId != null)
            {
                appDbContext = _context.Categories.Where(p => p.CategoryId == filterId);
                ViewBag.filterId = filterId;
            }
            else
            {
                appDbContext = _context.Categories;
            }

            IQueryable<Category> data;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                data = appDbContext
                        .Where(d => (d.Title.Contains(searchString))).OrderByDescending(d => d.CategoryId);
                ViewBag.searchString = searchString;
                if (data.Count() <= 0)
                {
                    TempData["Messages"] = "No data are available.";
                }
            }
            else
            {
                data = appDbContext.OrderByDescending(d => d.CategoryId);
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

            return View(dataInPage);
        }


        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> DetailsPartialView(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsCategory", category);
        }

        [HttpGet]
        public async Task<IActionResult> CheckCategoryExists(string title)
        {
            string slug = AppUtilities.GenerateSlug(title);
            var categoryExists = await _context.Categories.AnyAsync(c => c.Title == title || c.Slug == slug);
            return Json(categoryExists);
        }

        [HttpGet]
        public async Task<IActionResult> CheckCategoryUpdateExists(string title, int id)
        {
            string slug = AppUtilities.GenerateSlug(title);
            var categoryExists = await _context.Categories.AnyAsync(c => (c.Title == title || c.Slug == slug) && c.CategoryId != id);
            return Json(categoryExists);
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Title,Description,Slug")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Title))
            {
                Messages = "Can't add category";
                return RedirectToAction(nameof(Index));
            }

            category.Title = category.Title.Trim();
            if (category.Title.Length < 2)
            {
                Messages = "Can't add category";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                category.Slug = AppUtilities.GenerateSlug(category.Title);

                //var duplicationSlug = _context.Categories.Any(c => c.Slug == category.Slug);
                //var duplicationTitle = _context.Categories.Any(c => c.Title == category.Title);
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
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            Messages = "Can't add category";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditPartialView(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return PartialView("_EditCategory", category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Title,Description,Slug")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(category.Title))
            {
                Messages = "Can't edit category";
                return RedirectToAction(nameof(Index));
            }

            category.Title = category.Title.Trim();
            if (category.Title.Length < 2)
            {
                Messages = "Can't edit category";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Slug = AppUtilities.GenerateSlug(category.Title);
                    var duplicationSlug = _context.Categories.Any(c => c.Slug == category.Slug && c.CategoryId != id);
                    var duplicationTitle = _context.Categories.Any(c => c.Title == category.Title && c.CategoryId != id);
                    if (duplicationTitle)
                    {
                        ModelState.AddModelError("", "Title has already existed.");
                        Messages = "Title has already existed.";
                        return RedirectToAction(nameof(Index));
                    }
                    if (duplicationSlug)
                    {
                        ModelState.AddModelError("", "Slug has already existed.");
                        Messages = "Slug has already existed.";
                        return RedirectToAction(nameof(Index));
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            Messages = "Can't edit category";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> DeletePartialView(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteCategory", category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
