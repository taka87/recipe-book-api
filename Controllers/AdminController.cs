using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeBookApi.Models;

namespace RecipeBookApi.Controllers
{
    [Authorize(Roles = "admin")] // Само за администратори
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            //Console.WriteLine("🔴 ВЛИЗАМЕ В GET USERS МЕТОДА");
            var users = _dbContext.Users
                .Select(u => new { u.Id, u.FirstName, u.Email, u.Role })
                .ToList();
            //return Ok(new { msg = "Работи!" });
            return Ok(users);
        }

        [HttpGet("recipes")]
        public IActionResult GetRecipes()
        {
            var recipes = _dbContext.user_recipes
                .Join(_dbContext.Users, r => r.UserId, u => u.Id, (r, u) => new
                {
                    Id = r.Id,  // ✅ Връщаме ID на рецептата
                    Author = u.FirstName,
                    r.RecipeName,
                    Ingredients = r.Ingredients.Length > 30 ? r.Ingredients.Substring(0, 30) + "..." : r.Ingredients,
                    Description = r.Description.Length > 30 ? r.Description.Substring(0, 30) + "..." : r.Description
                })
                .ToList();

            return Ok(recipes);
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] User user)
        {
            // 🔍 1️⃣ Проверка дали потребителят вече съществува
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Потребителят вече съществува!");
            }

            // 🔹 2️⃣ Хеширане на паролата
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.Role = "admin"; // 👈 Задаваме роля "admin"

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // 🔹 3️⃣ Генериране на токен
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ТВОЯТ_СЕКРЕТЕН_КЛЮЧ");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, "admin") // 👈 Тук добавяме ролята в токена
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { user.Id, user.FirstName, Token = tokenString });
        }
    }
}
