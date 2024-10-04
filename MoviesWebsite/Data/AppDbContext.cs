using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Models;
using MoviesWebsite.Models.Movie;
using System.Reflection.Emit;

namespace MoviesWebsite.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Evaluate> Evaluates { get; set; }
        public DbSet<Favourite> Favourite { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<ImageMovie> ImageMovies { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = item.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    item.SetTableName(tableName.Substring(6));// or Replace("AspNet","")
                }
            }

            modelBuilder.Entity<Category>().Property(c => c.Slug).IsRequired();
            modelBuilder.Entity<Episode>().Property(c => c.Slug).IsRequired();
            modelBuilder.Entity<Movie>().Property(c => c.Slug).IsRequired();
            modelBuilder.Entity<Movie>().Property(c => c.CreatedDate).IsRequired();
            modelBuilder.Entity<Movie>().Property(c => c.UpdateDate).IsRequired();
            modelBuilder.Entity<Comment>().Property(c => c.MovieId).IsRequired();
            modelBuilder.Entity<Comment>().Property(c => c.UserId).IsRequired();
            modelBuilder.Entity<Comment>().Property(c => c.CreatedDate).IsRequired();
            modelBuilder.Entity<Comment>().Property(c => c.UpdateDate).IsRequired();
            modelBuilder.Entity<Evaluate>().Property(c => c.MovieId).IsRequired();
            modelBuilder.Entity<Evaluate>().Property(c => c.UserId).IsRequired();
            modelBuilder.Entity<Favourite>().Property(c => c.MovieId).IsRequired();
            modelBuilder.Entity<Favourite>().Property(c => c.UserId).IsRequired();
            modelBuilder.Entity<History>().Property(c => c.UserId).IsRequired();
            modelBuilder.Entity<History>().Property(c => c.EpisodeId).IsRequired();

            modelBuilder.Entity<Evaluate>()
                .HasOne(e => e.Movie)
                .WithMany(m => m.Evaluates)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evaluate>()
                .HasOne(e => e.User)
                .WithMany(u => u.Evaluates)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favourite>()
                .HasOne(e => e.Movie)
                .WithMany(m => m.Favourites)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favourite>()
                .HasOne(e => e.User)
                .WithMany(u => u.Favourites)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<History>()
	            .HasOne(e => e.Episode)
	            .WithMany(m => m.Histories)
	            .OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<History>()
                .HasOne(e => e.User)
                .WithMany(u => u.Histories)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Episode>()
                .HasOne(e => e.Movie)
                .WithMany(m => m.Episodes)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ImageMovie>()
                .HasOne(i => i.Movie)
                .WithMany(m => m.ImageMovies)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
