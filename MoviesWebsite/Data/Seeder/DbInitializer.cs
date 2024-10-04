using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;

namespace MoviesWebsite.Data.Seeder
{
    public class DbInitializer
    {
        public static async Task Initialize(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure the database is created
            context.Database.EnsureCreated();

            // Clear existing data
            if (await context.Actors.AnyAsync())
            {
                context.Actors.RemoveRange(context.Actors);
                await context.SaveChangesAsync();
            }

            // Seed roles
            if (!await roleManager.Roles.AnyAsync())
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Seed default users
            if (await userManager.Users.AnyAsync())
            {
                return; // DB has been seeded
            }

            var adminUser = new AppUser
            {
                UserName = "admin@movieswebsite.com",
                Email = "admin@movieswebsite.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@12345");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new Exception("Failed to create default admin user.");
            }

            // Seed additional users
            var users = new AppUser[]
            {
        new AppUser { UserName = "user1@movieswebsite.com", Email = "user1@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user2@movieswebsite.com", Email = "user2@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user3@movieswebsite.com", Email = "user3@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user4@movieswebsite.com", Email = "user4@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user5@movieswebsite.com", Email = "user5@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user6@movieswebsite.com", Email = "user6@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user7@movieswebsite.com", Email = "user7@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user8@movieswebsite.com", Email = "user8@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user9@movieswebsite.com", Email = "user9@movieswebsite.com", EmailConfirmed = true },
        new AppUser { UserName = "user10@movieswebsite.com", Email = "user10@movieswebsite.com", EmailConfirmed = true }
            };

            foreach (var user in users)
            {
                var userResult = await userManager.CreateAsync(user, "Password@123");
                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    throw new Exception($"Failed to create user {user.UserName}.");
                }
            }

            // Seed Groups
            var groups = new Group[]
            {
        new Group { Name = "ActionGr" },
        new Group { Name = "DramaGr" },
        new Group { Name = "ComedyGr" },
        new Group { Name = "HorrorGr" },
        new Group { Name = "Sci-FiGr" },
        new Group { Name = "DocumentaryGr" },
        new Group { Name = "AdventureGr" },
        new Group { Name = "FantasyGr" },
        new Group { Name = "MysteryGr" },
        new Group { Name = "BiographyGr" }
            };
            context.Groups.AddRange(groups);

            // Seed Categories
            var categories = new Category[]
            {
        new Category { Title = "Thriller", Slug = "thrillerSl" },
        new Category { Title = "Romance", Slug = "romanceSl" },
        new Category { Title = "Action", Slug = "actionSl" },
        new Category { Title = "Adventure", Slug = "adventureSl" },
        new Category { Title = "Horror", Slug = "horrorSl" },
        new Category { Title = "Drama", Slug = "dramaSl" },
        new Category { Title = "Fantasy", Slug = "fantasySl" },
        new Category { Title = "Comedy", Slug = "comedySl" },
        new Category { Title = "Mystery", Slug = "mysterySl" },
        new Category { Title = "Biography", Slug = "biographySl" }
            };
            context.Categories.AddRange(categories);

            // Seed Actors
            var actors = new Actor[]
            {
        new Actor { Name = "Tom Cruise", Role = "Lead" },
        new Actor { Name = "Scarlett Johansson", Role = "Supporting" },
        new Actor { Name = "Leonardo DiCaprio", Role = "Lead" },
        new Actor { Name = "Jennifer Lawrence", Role = "Supporting" },
        new Actor { Name = "Brad Pitt", Role = "Lead" },
        new Actor { Name = "Natalie Portman", Role = "Supporting" },
        new Actor { Name = "Johnny Depp", Role = "Lead" },
        new Actor { Name = "Emma Stone", Role = "Supporting" },
        new Actor { Name = "Chris Hemsworth", Role = "Lead" },
        new Actor { Name = "Margot Robbie", Role = "Supporting" }
            };
            context.Actors.AddRange(actors);

            // Save changes for Groups, Categories, and Actors
            await context.SaveChangesAsync();

            // Seed Movies
            var movies = new Movie[]
            {
        new Movie { Title = "Mission Impossible", Slug = "mission-impossible", Trailer = "https://youtube.com/trailer1", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, ReleaseDate = new DateTime(2024, 07, 01), Published = true, GroupId = groups[0].GroupId, Categories = new List<Category> { categories[0], categories[2] }, Actors = new List<Actor> { actors[0], actors[2] } },
        new Movie { Title = "Lost in Translation", Slug = "lost-in-translation", Trailer = "https://youtube.com/trailer2", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, ReleaseDate = new DateTime(2024, 08, 01), Published = true, GroupId = groups[1].GroupId, Categories = new List<Category> { categories[1], categories[5] }, Actors = new List<Actor> { actors[1], actors[3] } },
        new Movie { Title = "Inception", Slug = "inception", Trailer = "https://youtube.com/trailer3", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, ReleaseDate = new DateTime(2024, 09, 01), Published = true, GroupId = groups[4].GroupId, Categories = new List<Category> { categories[6], categories[4] }, Actors = new List<Actor> { actors[2], actors[4] } },
        new Movie { Title = "The Wolf of Wall Street", Slug = "the-wolf-of-wall-street", Trailer = "https://youtube.com/trailer4", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, ReleaseDate = new DateTime(2024, 10, 01), Published = true, GroupId = groups[9].GroupId, Categories = new List<Category> { categories[5], categories[8] }, Actors = new List<Actor> { actors[4], actors[6] } },
        new Movie { Title = "Titanic", Slug = "titanic", Trailer = "https://youtube.com/trailer5", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, ReleaseDate = new DateTime(2024, 11, 01), Published = true, GroupId = groups[2].GroupId, Categories = new List<Category> { categories[1], categories[9] }, Actors = new List<Actor> { actors[0], actors[7] } },
        new Movie { Title = "The Avengers", Slug = "the-avengers", Trailer = "https://youtube.com/trailer6", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, ReleaseDate = new DateTime(2024, 12, 01), Published = true, GroupId = groups[6].GroupId, Categories = new List<Category> { categories[2], categories[3] }, Actors = new List<Actor> { actors[6], actors[7] } }
            };
            context.Movies.AddRange(movies);

            // Save changes for Movies
            await context.SaveChangesAsync();

            // Seed Episodes
            var episodes = new Episode[]
            {
        new Episode { Title = "Episode 1", Slug = "episode-1", Link = "https://streaming.com/mission-impossible/episode1", MovieId = movies[0].MovieId },
        new Episode { Title = "Episode 2", Slug = "episode-2", Link = "https://streaming.com/lost-in-translation/episode2", MovieId = movies[1].MovieId },
        new Episode { Title = "Episode 3", Slug = "episode-3", Link = "https://streaming.com/inception/episode3", MovieId = movies[2].MovieId }
            };
            context.Episodes.AddRange(episodes);

            // Seed Comments
            var comments = new Comment[]
            {
        new Comment { Content = "Great movie!", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, MovieId = movies[0].MovieId, UserId = userManager.Users.First().Id },
        new Comment { Content = "I loved it!", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, MovieId = movies[1].MovieId, UserId = userManager.Users.Skip(1).First().Id },
        new Comment { Content = "Not bad.", CreatedDate = DateTime.Now, UpdateDate = DateTime.Now, MovieId = movies[2].MovieId, UserId = userManager.Users.Skip(2).First().Id }
            };
            context.Comments.AddRange(comments);

            // Seed Evaluates
            var evaluates = new Evaluate[]
            {
        new Evaluate { Star = 4, MovieId = movies[0].MovieId, UserId = userManager.Users.First().Id },
        new Evaluate { Star = 5, MovieId = movies[1].MovieId, UserId = userManager.Users.Skip(1).First().Id },
        new Evaluate { Star = 3, MovieId = movies[2].MovieId, UserId = userManager.Users.Skip(2).First().Id }
            };
            context.Evaluates.AddRange(evaluates);

            // Save all changes
            await context.SaveChangesAsync();
        }

    }
}
