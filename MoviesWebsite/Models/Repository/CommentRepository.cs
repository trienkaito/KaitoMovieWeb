using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Models.Repository
{
    public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }
    }

}
