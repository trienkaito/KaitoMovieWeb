using Microsoft.EntityFrameworkCore;
using MoviesWebsite.Data;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;
using System.Linq.Expressions;

namespace MoviesWebsite.Models.Repository
{
	public class ActorRepository : RepositoryBase<Actor>, IActorRepository
	{
		public ActorRepository(AppDbContext context) : base(context)
		{
		}

		public async Task<Actor> FindActorIdAsync(int id)
		{
			return await _context.Actors
				.Include(a => a.Movies) // Eager load the Movies collection
				.FirstOrDefaultAsync(a => a.ActorId == id);
		}

		public async Task<Actor> GetExistAsync(Expression<Func<Actor, bool>> predicate)
		{
			return await _context.Actors.FirstOrDefaultAsync(predicate);
		}
	}
}
