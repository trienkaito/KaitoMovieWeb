using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using MVC.Utilities;
using NuGet.DependencyResolver;
using OfficeOpenXml;


namespace MVc.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/admin/database-manage/{action=Index}")]
    public class DBManage : Controller
    {
        private readonly AppDbContext _dBContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<DBManage> _logger;

		public DBManage(AppDbContext appDBContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, AppDbContext dBContext, ILogger<DBManage> logger)
		{
			_dBContext = appDBContext;
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
			_dBContext = dBContext;
			_logger = logger;
		}

		[TempData]
        public string Messages { get; set; }

        // GET: DBManage
        public async Task<ActionResult> Index()
        {
            var hidden = true;
            try
            {
                if(_signInManager.IsSignedIn(this.User))
                {
                    var user = await _userManager.GetUserAsync(this.User);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        hidden = !roles.Any(r => r == RoleName.Administrator);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            ViewData["hidden"] = hidden;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult DeleteDB()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteDBAsync()
        {
            var success = await _dBContext.Database.EnsureDeletedAsync();
            Messages = success ? "" : "Can't delete Database.";
            if(success)
            {
               await _signInManager.SignOutAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApplyMigrationAsync()
        {
            await _dBContext.Database.MigrateAsync();
            Messages = "Update Database success.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendDataAsync()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var r in roleNames)
            {
                var roleName = r.GetRawConstantValue() as string;
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // edit
            var edit = await _userManager.FindByNameAsync("edit");
            if (edit == null)
            {
                edit = new AppUser()
                {
                    UserName = "edit",
                    Email = "edit@example.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(edit, password: "edit123");
                await _userManager.AddToRoleAsync(edit, RoleName.Editor);
            }

            // member
            var premium = await _userManager.FindByNameAsync("premium");
            if (premium == null)
            {
                premium = new AppUser()
                {
                    UserName = "premium",
                    Email = "premium@example.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(premium, password: "premium123");
                await _userManager.AddToRoleAsync(premium, RoleName.Premium);
            }

            // user
            for(int i = 0; i<10;i++)
            {
                var user = await _userManager.FindByNameAsync($"user{i}");
                if (user == null)
                {
                    user = new AppUser()
                    {
                        UserName = $"user{i}",
                        Email = $"user{i}@example.com",
                        EmailConfirmed = true,
                    };

                    await _userManager.CreateAsync(user, password: "123456");
                }
            }

            // admin, pass = admin123, email = admin@example.com
            var admin = await _userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };

                await _userManager.CreateAsync(admin, password: "admin123");
                await _userManager.AddToRoleAsync(admin, RoleName.Administrator);
                await _signInManager.SignInAsync(admin, false);

                return RedirectToAction("SendData");

            }
            else
            {
                var user = await _userManager.GetUserAsync(this.User);
                if (user == null) return this.Forbid();
                var roles = await _userManager.GetRolesAsync(user);

                if (!roles.Any(r => r == RoleName.Administrator))
                {
                    return this.Forbid();
                }

            }

            await DeteleSeedData();
            await SeedCategory();
            await SeedActor();
            await SeedMovie();
            await SeedEpisode();

            Messages = "Send data success";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeteleSeedDataAsync()
        {
            await DeteleSeedData();
            Messages = "Delete seeddata success";
            return RedirectToAction("Index");
        }

        private async Task DeleteComment(Comment comment)
        {
            if(comment == null)
            {
                return;
            }

            if (comment.CommentChildren != null)
            {
                foreach (var item in comment.CommentChildren)
                {
                    await DeleteComment(item);
                }
            }

            _dBContext.Remove(comment);
        }
        private async Task DeteleSeedData()
        {
            try
            {
                _dBContext.Actors.RemoveRange(_dBContext.Actors.Where(c => c.Name.EndsWith("`")));
                _dBContext.Categories.RemoveRange(_dBContext.Categories.Where(c => c.Description != null && c.Description.EndsWith("`")));
                var movie = _dBContext.Movies.Include(m => m.Comments).Where(c => c.Description != null && c.Description.EndsWith("`"));
                foreach (var item in movie)
                {
                    foreach(var comment in item.Comments)
                    {
                        await DeleteComment(comment);
                    }
                }
                _dBContext.Movies.RemoveRange(movie);

            }
            catch (Exception ex)
            {
                _logger.LogError("Delte seed data: " + ex.Message);
            }

            await _dBContext.SaveChangesAsync();
            return;
        }

        private async Task SeedCategory()
        {
            List<Category> categories = new List<Category>();

            try
            {
				using (var package = new ExcelPackage(new FileInfo("Uploads/data/SendData.xlsx")))
				{
					var worksheet = package.Workbook.Worksheets["Category"]; // Lấy sheet

					// Lặp qua các hàng và cột
					// bat dau tu 1
					for (int row = 2; row <= worksheet.Dimension.Rows; row++)
					{
                        try
                        {
                            var title = worksheet.Cells[row, 1].Text;
                            var description = worksheet.Cells[row, 2].Text;

                            if (string.IsNullOrWhiteSpace(description))
                            {
                                description = null;
                            }

                            if (!string.IsNullOrWhiteSpace(title))
                            {
                                categories.Add(new Category()
                                {
                                    Title = title,
                                    Description = description + "`",
                                    Slug = AppUtilities.GenerateSlug(title)
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Seed Category: " + ex.Message);
                        }              
					}

					await _dBContext.AddRangeAsync(categories);
					await _dBContext.SaveChangesAsync();
				}
			}
            catch (Exception ex)
            {
                _logger.LogError("Seed Category: "+ ex.Message);
            }

        }

        private async Task SeedActor()
        {
            List<Actor> actors = new List<Actor>();

            try
			{
				using (var package = new ExcelPackage(new FileInfo("Uploads/data/SendData.xlsx")))
				{
					var worksheet = package.Workbook.Worksheets["Actor"]; // Lấy sheet

					// Lặp qua các hàng và cột
					// bat dau tu 1
					for (int row = 2; row <= worksheet.Dimension.Rows; row++)
					{
                        try
                        {
                            var name = worksheet.Cells[row, 1].Text;
                            var role = worksheet.Cells[row, 2].Text;
                            var image = worksheet.Cells[row, 3].Text;

                            if (string.IsNullOrWhiteSpace(image))
                            {
                                image = null;
                            }

                            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(role))
                            {
                                actors.Add(new Actor()
                                {
                                    Name = name + "`",
                                    Role = role,
                                    Image = image
                                });
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError("Seed Actor: " + ex.Message);
                        }
						
					}

					await _dBContext.AddRangeAsync(actors);
					await _dBContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Seed Actor: " + ex.Message);
			}

        }

		private async Task SeedMovie()
		{
			List<Movie> movies = new List<Movie>();

            try
			{
				using (var package = new ExcelPackage(new FileInfo("Uploads/data/SendData.xlsx")))
				{
					var worksheet = package.Workbook.Worksheets["Movie"]; // Lấy sheet

					// Lặp qua các hàng và cột
					// bat dau tu 1
					for (int row = 2; row <= worksheet.Dimension.Rows; row++)
					{
                        try
                        {

                            var title = worksheet.Cells[row, 1].Text;
                            var description = worksheet.Cells[row, 2].Text;
                            var releaseDate = worksheet.Cells[row, 3].Text;
                            var published = worksheet.Cells[row, 4].Text;
                            var image = worksheet.Cells[row, 5].Text;
                            var series = worksheet.Cells[row, 6].Text;
                            var trailer = worksheet.Cells[row, 7].Text;
                            var actor = worksheet.Cells[row, 8].Text;
                            var director = worksheet.Cells[row, 9].Text;
                            var category = worksheet.Cells[row, 10].Text;

                            if (string.IsNullOrWhiteSpace(image))
                            {
                                image = null;
                            }

                            // release date
                            bool checkReleasaDate = false;
                            DateTime release = new DateTime();
                            if (!string.IsNullOrWhiteSpace(releaseDate))
                            {
                                
                                if (DateTime.TryParse(releaseDate, out release))
                                {
                                    checkReleasaDate = true;
                                }
                            }

                            // publisher
                            bool checkPulished = false;
                            bool publish = false;
                            if (!string.IsNullOrWhiteSpace(published))
                            {

                                if (bool.TryParse(published, out publish))
                                {
                                    checkPulished = true;
                                }
                            }

                            // series
                            bool checkSeries = false;
                            bool seri = false;
                            if (!string.IsNullOrWhiteSpace(series))
                            {

                                if (bool.TryParse(series, out seri))
                                {
                                    checkSeries = true;
                                }
                            }

                            // actor
                            List<Actor> actors = new List<Actor>();
                            if(!string.IsNullOrWhiteSpace(actor))
                            {
                                var split = actor.Split(',',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
                                if (split.Length > 0)
                                {
                                    foreach (var item in split)
                                    {
                                        var temp = _dBContext.Actors.Where(a =>  a.Name == item+"`" && a.Role=="Actor").FirstOrDefault();
                                        if(temp != null)
                                        {
                                            actors.Add(temp);
                                        }
                                    }
                                    
                                }
                            }
                            // director
                            if (!string.IsNullOrWhiteSpace(director))
                            {
                                var split = director.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                if (split.Length > 0)
                                {
                                    foreach (var item in split)
                                    {
                                        var temp = _dBContext.Actors.Where(a => a.Name == item + "`" && a.Role == "Director").FirstOrDefault();
                                        if (temp != null)
                                        {
                                            actors.Add(temp);
                                        }
                                    }

                                }
                            }

                            // category
                            List<Category> categories = new List<Category>();
                            if (!string.IsNullOrWhiteSpace(category))
                            {
                                var split = category.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                if (split.Length > 0)
                                {
                                    foreach (var item in split)
                                    {
                                        var temp = _dBContext.Categories.Where(a => a.Title == item ).FirstOrDefault();
                                        if (temp != null)
                                        {
                                            categories.Add(temp);
                                        }
                                    }

                                }
                            }

                            if (!string.IsNullOrWhiteSpace(title) 
                                && !string.IsNullOrWhiteSpace(trailer))
                            {

                                movies.Add(new Movie()
                                {
                                    Title = title,
                                    Description = description + "`",
                                    CreatedDate = DateTime.Now,
                                    UpdateDate = DateTime.Now,
                                    ReleaseDate = checkReleasaDate ? release : null,
                                    Published = checkPulished ? publish : false,
                                    Image = image,
                                    Series = checkSeries ? seri : false,
                                    Trailer = trailer,
                                    Actors = actors.Count > 0 ? actors : null,
                                    Categories = categories.Count > 0 ? categories : null,
                                    Slug = AppUtilities.GenerateSlug(title)
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Seed Movies: " + ex.Message);
                        }

					}

					await _dBContext.AddRangeAsync(movies);
					await _dBContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Seed Movies: " + ex.Message);
			}

		}

        private async Task SeedEpisode()
        {
            List<Episode> episodes = new List<Episode>();

            try
            {
                using (var package = new ExcelPackage(new FileInfo("Uploads/data/SendData.xlsx")))
                {
                    var worksheet = package.Workbook.Worksheets["Episode"]; // Lấy sheet

                    // Lặp qua các hàng và cột
                    // bat dau tu 1
                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        try
                        {

                            var movieTitle = worksheet.Cells[row, 1].Text;
                            var episodeTitle = worksheet.Cells[row, 2].Text;
                            var link = worksheet.Cells[row, 3].Text;


                            if (!string.IsNullOrWhiteSpace(movieTitle)
                                && !string.IsNullOrWhiteSpace(episodeTitle)
                                && !string.IsNullOrWhiteSpace(link))
                            {
                                var movie = await _dBContext.Movies.FirstOrDefaultAsync(m =>  m.Title == movieTitle.Trim());
                                if(movie != null)
                                {
                                    episodes.Add(new Episode()
                                    {
                                        MovieId = movie.MovieId,
                                        Title = episodeTitle,
                                        Link = link,
                                        Slug = AppUtilities.GenerateSlug(episodeTitle)
                                    });
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Seed Episode: " + ex.Message);
                        }

                    }

                    await _dBContext.AddRangeAsync(episodes);
                    await _dBContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Seed Episode: " + ex.Message);
            }

        }
    }
}
