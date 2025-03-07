using System.ComponentModel.DataAnnotations;

namespace RecipeBookApi.Models
{
    public class RecipeDto
    {
        [Required]
        public string RecipeName { get; set; } = string.Empty;

        [Required]
        public string Ingredients { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
