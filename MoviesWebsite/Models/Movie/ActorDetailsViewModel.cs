namespace MoviesWebsite.Models.Movie
{
	public class ActorDetailsViewModel
	{
		public Actor Actor { get; set; }
		public List<Movie> Movies { get; set; }
		public List<Movie> LatestMovies { get; set; } // New property for the latest movies
	}
}
