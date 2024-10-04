using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using MoviesWebsite.Models;
using System.Text;

namespace MVC.Menu
{
    public class AdminSidebarService
    {
        private readonly IUrlHelper UrlHelper;
        public List<SidebarItem> Items { get; set; } = new List<SidebarItem>();

        public AdminSidebarService(IUrlHelperFactory factory, IActionContextAccessor action, UserManager<AppUser> userManager)
        {
            UrlHelper = factory.GetUrlHelper(action.ActionContext);

            // Khoi tao cac muc sidebar

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Heading, Title = "General Manage" });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "Dashboard",
                Action = "Index",
                Area = "Admin",
                Title = "Dashboard",
                AwesomeIcon = "fas fa-tachometer-alt"
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "DBManage",
                Action = "Index",
                Area = "Database",
                Title = "Database Manage",
                AwesomeIcon = "fas fa-database"
            });

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Decentralization & Membership",
                AwesomeIcon = "far fa-folder",
                collapseID = "role",
                Items = new List<SidebarItem>() {
                        //new SidebarItem() { 
                        //        Type = SidebarItemType.NavItem,
                        //        Controller = "Role",
                        //        Action = "Index", 
                        //        Area = "Identity",
                        //        Title = "Role"
                        //},
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "User",
                                Action = "Index",
                                Area = "Identity",
                                Title = "Member List"
                        },
                    },
            });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Movie manage",
                AwesomeIcon = "far fa-folder",
                collapseID = "movie",
                Items = new List<SidebarItem>() {
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "Movie",
                                Action = "Index",
                                Area = "Admin",
                                Title = "Movie"
                        },
                         new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "categories",
                                Action = "Index",
                                Area = "Admin",
                                Title = "Category"
                        },
                        new SidebarItem() {
                                Type = SidebarItemType.NavItem,
                                Controller = "Actor",
                                Action = "Index",
                                Area = "Admin",
                                Title = "Actor"
                        },
                    },
            });

        }


        public string renderHtml()
        {
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(UrlHelper));
            }


            return html.ToString();
        }

        public void SetActive(string Controller, string Action, string Area, bool hidden = false)
        {

            foreach (var item in Items)
            {
                if ((item.Title == "Decentralization & Membership" || item.Title == "Database Manage") && hidden)
                {
                    item.IsHidden = true;
                }
            }

            foreach (var item in Items)
            {
                if (item.Controller == Controller && item.Action == Action && item.Area == Area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var childItem in item.Items)
                        {
                            if (childItem.Controller == Controller && childItem.Action == Action && childItem.Area == Area)
                            {
                                childItem.IsActive = true;
                                item.IsActive = true;
                                return;

                            }
                        }
                    }
                }
            }
        }
    }
}