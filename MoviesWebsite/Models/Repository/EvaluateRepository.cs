using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Models.Repository
{
    public class EvaluateRepository : RepositoryBase<Evaluate>, IEvaluateRepository
    {
        public EvaluateRepository(AppDbContext context) : base(context)
        {
        }
    }

}
