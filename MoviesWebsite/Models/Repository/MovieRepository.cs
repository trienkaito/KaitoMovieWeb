using MoviesWebsite.Data;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Models.Repository
{
    public class MovieRepository : RepositoryBase<Movie.Movie>, IMovieRepository
    {
        public MovieRepository(AppDbContext context) : base(context)
        {
        }
    }
}
