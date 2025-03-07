using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeBookApi.Models;

namespace RecipeBookApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public RecipeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        //[HttpGet("recipes")]
        //public IActionResult GetRecipes()
        //{
        //    var recipes = _dbContext.user_recipes
        //        .Select(r => new { r.Id, r.UserId, r.RecipeName, r.Ingredients, r.Description })
        //        .ToList();
        //    return Ok(recipes);
        //}

        [Authorize]
        [HttpGet("user-recipes")]
        public IActionResult GetUserRecipes()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Невалиден потребител!" });
            }

            var recipes = _dbContext.user_recipes
                .Where(r => r.UserId == userId) // 🔥 Взимаме само рецептите на логнатия потребител
                .Select(r => new { r.Id, r.RecipeName, r.Ingredients, r.Description })
                .ToList();

            return Ok(recipes);
        }

        //тест за токена защо дава 401
        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { message = "Токенът работи!", userId });
        }

        [Authorize] // Изискваме логнат потребител
        [HttpPost]
        public async Task<IActionResult> AddRecipe([FromBody] RecipeDto recipeDto)
        {
            if (recipeDto == null || string.IsNullOrWhiteSpace(recipeDto.RecipeName) ||
                string.IsNullOrWhiteSpace(recipeDto.Ingredients) || string.IsNullOrWhiteSpace(recipeDto.Description))
            {
                return BadRequest("Некоректни или липсващи данни за рецептата.");
            }

            // Взимаме `user_id` от токена (ИЗПОЛЗВАМЕ ClaimTypes.NameIdentifier)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //var claims = User.Claims.Select(c => new { c.Type, c.Value });
            //return Unauthorized(new { message = "Claims debug", claims });

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Проблем с потребителския идентификатор.");
            }

            int userId = int.Parse(userIdClaim);

            var recipe = new Recipe
            {
                UserId = userId,
                RecipeName = recipeDto.RecipeName,
                Ingredients = recipeDto.Ingredients,
                Description = recipeDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.user_recipes.Add(recipe);
            //_dbContext.UserRecipes.Add(recipe);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Рецептата е добавена успешно!" });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditRecipe(int id, [FromBody] Recipe updatedRecipe)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Невалиден потребител!" });
            }

            var recipe = await _dbContext.user_recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound(new { message = "Рецептата не е намерена!" });
            }

            // 🛑 Ако не е админ и не е собственик на рецептата -> Грешка 403
            if (recipe.UserId != userId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            // Обновяваме рецептата
            recipe.RecipeName = updatedRecipe.RecipeName;
            recipe.Ingredients = updatedRecipe.Ingredients;
            recipe.Description = updatedRecipe.Description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Рецептата е редактирана успешно!" });
        }

        //[Authorize]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> EditRecipe(int id, [FromBody] Recipe updatedRecipe)
        //{
        //    var recipe = await _dbContext.user_recipes.FindAsync(id);
        //    if (recipe == null)
        //    {
        //        return NotFound(new { message = "Рецептата не е намерена!" });
        //    }

        //    //recipe.Title         = updatedRecipe.Title;
        //    recipe.RecipeName = updatedRecipe.RecipeName;
        //    recipe.Ingredients = updatedRecipe.Ingredients;
        //    recipe.Description = updatedRecipe.Description;

        //    await _dbContext.SaveChangesAsync();

        //    return Ok(new { message = "Рецептата е редактирана успешно!" });
        //}



        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _dbContext.user_recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound(new { message = "Рецептата не е намерена!" });
            }

            _dbContext.user_recipes.Remove(recipe);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Рецептата е изтрита успешно!" });
        }
    }
}