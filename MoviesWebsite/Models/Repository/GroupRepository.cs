using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Models.Repository
{
    public class GroupRepository : RepositoryBase<Group>, IGroupRepository
    {
        public GroupRepository(AppDbContext context) : base(context)
        {
        }
    }
}
