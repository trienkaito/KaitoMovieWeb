using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;
using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Utilities
{
    public class UniqueActorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var actor = (Actor)validationContext.ObjectInstance;
            var _actorRepository = (IActorRepository)validationContext.GetService(typeof(IActorRepository));

            var existingActor = _actorRepository.GetExistAsync(a => a.Name == actor.Name && a.Role == actor.Role).Result;

            if (existingActor != null)
            {
                return new ValidationResult("Actor already exists in the system.", new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
