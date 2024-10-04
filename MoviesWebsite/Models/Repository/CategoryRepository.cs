using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Models.Repository
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
    }

}
