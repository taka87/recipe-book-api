using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeBookApi.Models
{
    public class Recipe
    {
        public int Id { get; set; } // ID (автоинкремент)

        [Column("user_id")]
        public int UserId { get; set; } // ID на потребителя, който създава рецептата
        
        [Column("recipe_name")]
        public string RecipeName { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;


        [Column("created_at")]
        public DateTime CreatedAt { get; set; } // 🔑 Премахни "required"
    }
}