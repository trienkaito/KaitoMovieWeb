using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Models.Repository
{
    public class EpisodeRepository : RepositoryBase<Episode>, IEpisodeRepository
    {
        public EpisodeRepository(AppDbContext context) : base(context)
        {
        }
    }

}
