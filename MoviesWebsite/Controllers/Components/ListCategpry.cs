using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;

namespace MoviesWebsite.Controllers
{
    [ViewComponent]
    public class ListCategpry : ViewComponent
    {
        private readonly AppDbContext _context;

        public ListCategpry(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await _context.Categories.ToListAsync());
        }
    }
}
