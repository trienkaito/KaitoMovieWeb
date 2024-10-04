using MoviesWebsite.Models.Movie;
using System.Linq.Expressions;

namespace MoviesWebsite.Models.Repository.Interface
{
	public interface IActorRepository : IRepository<Actor>
	{
		Task<Actor> GetExistAsync(Expression<Func<Actor, bool>> predicate);
		Task<Actor> FindActorIdAsync(int id);

	}
}
