using Bogus.DataSets;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("History")]
    [Index(nameof(EpisodeId), nameof(UserId),nameof(Time))]
    public class History
    {
        [Key]
        public int HistoryId { get; set; }

		public int? EpisodeId { get; set; }

		[ForeignKey(nameof(EpisodeId))]
		public Episode? Episode { get; set; }

		public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        public DateTime? Time { get; set; }
    }
}
