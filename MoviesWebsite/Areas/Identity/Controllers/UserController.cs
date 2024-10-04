// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MoviesWebsite.Areas.Identity.Models.UserViewModels;
using MoviesWebsite.Data;
using MVC.ExtendMethods;
using MoviesWebsite.Models;
using MoviesWebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace MoviesWebsite.Areas.Identity.Controllers
{

    [Authorize(Roles = RoleName.Administrator)]
    [Area("Identity")]
    [Route("/ManageUser/[action]")]
    public class UserController : Controller
    {

        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }



        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /ManageUser/Index
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, string searchString)
        {
            var model = new UserListModel();


            model.currentPage = currentPage;

            IQueryable<AppUser> qr;
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                qr = _userManager.Users
                        .Where(d => (d.UserName.Contains(searchString))).OrderBy(d => d.UserName);
                ViewBag.searchString = searchString;
                if (qr.Count() <= 0)
                {
                    TempData["Messages"] = "No data are available.";
                    model.users = new List<UserAndRole>();
                    return View(model);
                }
            }
            else
            {
                qr = _userManager.Users.OrderBy(u => u.UserName);
            }

            model.totalUsers = await qr.CountAsync();
            model.countPages = (int)Math.Ceiling((double)model.totalUsers / model.ITEMS_PER_PAGE);

            if (model.currentPage < 1)
                model.currentPage = 1;
            if (model.currentPage > model.countPages)
                model.currentPage = model.countPages;

            var qr1 = qr.Skip((model.currentPage - 1) * model.ITEMS_PER_PAGE)
                        .Take(model.ITEMS_PER_PAGE)
                        .Select(u => new UserAndRole()
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                        });

            model.users = await qr1.ToListAsync();

            foreach (var user in model.users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.RoleNames = string.Join(",", roles);
            }

            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {
            // public SelectList allRoles { get; set; }
            var model = new AddUserRoleModel();
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"User not found");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"User not found, id = {id}.");
            }

            model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray<string>();

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.allRoles = new SelectList(roleNames);

            await GetClaims(model);

            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, [Bind("RoleNames")] AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"User not found");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"User not found, id = {id}.");
            }
            await GetClaims(model);

            var OldRoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();
            IEnumerable<string> deleteRoles = OldRoleNames;
            IEnumerable<string> addRoles = new List<string>();

            if(model.RoleNames !=null)
            {
                deleteRoles = OldRoleNames.Where(r => !model.RoleNames.Contains(r));
                addRoles = model.RoleNames.Where(r => !OldRoleNames.Contains(r));
            }

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            ViewBag.allRoles = new SelectList(roleNames);

            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user, deleteRoles);
            if (!resultDelete.Succeeded)
            {
                ModelState.AddModelError(resultDelete);
                return View(model);
            }

            var resultAdd = await _userManager.AddToRolesAsync(model.user, addRoles);
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError(resultAdd);
                return View(model);
            }


            StatusMessage = $"Just update role for user: {model.user.UserName}";

            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> SetPasswordAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"User not found.");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"User not found, id = {id}.");
            }

            return View();
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPasswordAsync(string id, SetUserPasswordModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"User not found");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"User not found, id = {id}.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _userManager.RemovePasswordAsync(user);

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            StatusMessage = $"Just update password for user: {user.UserName}";

            return RedirectToAction("Index");
        }


        [HttpGet("{userid}")]
        public async Task<ActionResult> AddClaimAsync(string userid)
        {

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("User not found");
            ViewBag.user = user;
            return View();
        }

        [HttpPost("{userid}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddClaimAsync(string userid, AddUserClaimModel model)
        {

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("User not found");
            ViewBag.user = user;
            if (!ModelState.IsValid) return View(model);
            var claims = _context.UserClaims.Where(c => c.UserId == user.Id);

            if (claims.Any(c => c.ClaimType == model.ClaimType && c.ClaimValue == model.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, "Claim already exits");
                return View(model);
            }

            await _userManager.AddClaimAsync(user, new Claim(model.ClaimType, model.ClaimValue));
            StatusMessage = "Just add claim to user";

            return RedirectToAction("AddRole", new { id = user.Id });
        }

        [HttpGet("{claimid}")]
        public async Task<IActionResult> EditClaim(int claimid)
        {
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            var user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("User not found.");

            var model = new AddUserClaimModel()
            {
                ClaimType = userclaim.ClaimType,
                ClaimValue = userclaim.ClaimValue

            };
            ViewBag.user = user;
            ViewBag.userclaim = userclaim;
            return View("AddClaim", model);
        }
        [HttpPost("{claimid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClaim(int claimid, AddUserClaimModel model)
        {
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            var user = await _userManager.FindByIdAsync(userclaim.UserId);
            if (user == null) return NotFound("User not found");

            if (!ModelState.IsValid) return View("AddClaim", model);

            if (_context.UserClaims.Any(c => c.UserId == user.Id
                && c.ClaimType == model.ClaimType
                && c.ClaimValue == model.ClaimValue
                && c.Id != userclaim.Id))
            {
                ModelState.AddModelError("Claim already exits");
                return View("AddClaim", model);
            }


            userclaim.ClaimType = model.ClaimType;
            userclaim.ClaimValue = model.ClaimValue;

            await _context.SaveChangesAsync();
            StatusMessage = "You just update claim";


            ViewBag.user = user;
            ViewBag.userclaim = userclaim;
            return RedirectToAction("AddRole", new { id = user.Id });
        }
        [HttpPost("{claimid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaimAsync(int claimid)
        {
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            var user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("User not found");

            await _userManager.RemoveClaimAsync(user, new Claim(userclaim.ClaimType, userclaim.ClaimValue));

            StatusMessage = "You have delete claim";

            return RedirectToAction("AddRole", new { id = user.Id });
        }

        private async Task GetClaims(AddUserRoleModel model)
        {
            var listRoles = from r in _context.Roles
                            join ur in _context.UserRoles on r.Id equals ur.RoleId
                            where ur.UserId == model.user.Id
                            select r;

            var _claimsInRole = from c in _context.RoleClaims
                                join r in listRoles on c.RoleId equals r.Id
                                select c;
            model.claimsInRole = await _claimsInRole.ToListAsync();


            model.claimsInUserClaim = await (from c in _context.UserClaims
                                             where c.UserId == model.user.Id
                                             select c).ToListAsync();

        }

        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            if(user.UserName == "admin")
            {
                return this.Forbid();
            }

            var resultLockoutEnabled = await _userManager.SetLockoutEnabledAsync(user, true);
            var resultLockoutEndDate = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            var logoutUser = await _userManager.UpdateSecurityStampAsync(user);
            
            if (resultLockoutEnabled.Succeeded && resultLockoutEndDate.Succeeded && logoutUser.Succeeded)
            {
                TempData["Messages"] = $"Ban {user.UserName} success.";
            }
            else
            {
                TempData["Messages"] = $"Ban {user.UserName} fail.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UnLockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            var resultLockoutEnabled = await _userManager.SetLockoutEnabledAsync(user, true);
            var resultLockoutEndDate = await _userManager.SetLockoutEndDateAsync(user, null);

            if (resultLockoutEnabled.Succeeded && resultLockoutEndDate.Succeeded)
            {
                TempData["Messages"] = $"Unban {user.UserName} success.";
            }
            else
            {
                TempData["Messages"] = $"Unban {user.UserName} fail.";
            }
            return RedirectToAction("Index");
        }
    }
}
