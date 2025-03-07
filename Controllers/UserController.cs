using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using RecipeBookApi.Models;
using BCrypt.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace RecipeBookApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("API is working");

        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("")]
        public IActionResult GetApiStatus()
        {
            return Ok("API is working");
        }

        ////ТЕСТ->>  http://localhost:5000/api/user/check-db
        //[HttpGet("check-db")]
        //public IActionResult CheckDatabaseConnection()
        //{
        //    try
        //    {
        //        var usersCount = _dbContext.Users.Count();
        //        return Ok($"✅ Връзката е активна! Брой потребители: {usersCount}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"❌ Проблем с базата: {ex.Message}");
        //    }
        //}


        ////ТЕСТ->>  http://localhost:5000/api/user/test
        //[HttpGet("test")]
        //public IActionResult TestApi()
        //{
        //    return Ok("API is running");   //това съобщение се показва на 
        //}


        // GET: api/user
        [Authorize] // ⬅️ Това ще изисква токен
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            Console.WriteLine("🔍 GET /api/user/all-users заявка получена!");
            var users = _dbContext.Users.Select(u => new { u.Id, u.Email }).ToList();
            return Ok(users);
        }

        // GET: api/user/{id}
        [Authorize] // ⬅️ Това ще изисква токен
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound("Потребителят не съществува.");
            return Ok(user);
        }


        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = _dbContext.Users
                .Where(u => u.Id == int.Parse(userId))
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound("Потребителят не е намерен.");
            }

            return Ok(user);
        }




        //// POST: api/user
        //1 ->>
        //[HttpPost("register")]
        //public async Task<IActionResult> CreateUser(User user)
        //{
        //    await _dbContext.Users.AddAsync(user);
        //    await _dbContext.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        //}

        //2->
        //[HttpPost("register")]
        //public async Task<IActionResult> CreateUser([FromBody] User user)
        //{
        //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        //    user.CreatedAt = DateTime.UtcNow;
        //    await _dbContext.Users.AddAsync(user);
        //    await _dbContext.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        //}

        //3->

        //[Authorize]   //to test-> не може да иска оторизация при регистрация(низ с токен, трябва д аго иска при логин)
        //[HttpGet("protected-route")]
        //public IActionResult GetSecretData()
        //{
        //    return Ok("Това е защитена информация!");
        //}

        ////->> 4
        //[Authorize] // ⬅️ Изисква JWT токен
        //[HttpGet("protected-route")]
        //public IActionResult GetSecretData()
        //{
        //    return Ok("Това е защитена информация!");
        //}
        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] User user)
        //{
        //    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        //    user.CreatedAt = DateTime.UtcNow;

        //    await _dbContext.Users.AddAsync(user);
        //    await _dbContext.SaveChangesAsync();

        //    // Генерираме JWT токен за новия потребител
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.UTF8.GetBytes("ТВОЯТ_СУПЕР_СИГУРЕН_КЛЮЧ_12345");
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new[]
        //        {
        //    new Claim(ClaimTypes.Name, user.FirstName),
        //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        //}),
        //        Expires = DateTime.UtcNow.AddHours(2),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    var tokenString = tokenHandler.WriteToken(token);

        //    // Връщаме потребителя + токена
        //    return Ok(new { user.Id, user.FirstName, Token = tokenString });
        //}

        // ->>5
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // 🔍 1️⃣ Провери дали потребителят вече съществува
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Потребителят вече съществува!"); // 🚨 Връща 400 без токен
            }

            // 🔹 2️⃣ Ако не съществува -> хеширай паролата
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // 🔹 3️⃣ Генерирай JWT токен само ако всичко е наред
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ТВОЯТ_СУПЕР_СИГУРЕН_КЛЮЧ_12345");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { user.Id, user.FirstName, Token = tokenString });
        }


        //[HttpPost("register")]
        //public async Task<IActionResult> CreateUser([FromBody] User user)
        //{
        //    // Лог на получените данни
        //    Console.WriteLine($"Received user: {JsonSerializer.Serialize(user)}");

        //    // Премахни дублиращия се ModelState.IsValid
        //    if (!ModelState.IsValid)
        //    {
        //        Console.WriteLine($"ModelState errors: {JsonSerializer.Serialize(ModelState)}");
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        //        user.CreatedAt = DateTime.UtcNow;

        //        await _dbContext.Users.AddAsync(user);
        //        await _dbContext.SaveChangesAsync();

        //        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Database error: {ex.Message}");
        //        return StatusCode(500, "Грешка при запис в базата");
        //    }
        //}

        //[Authorize]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Невалиден email или парола!" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ТВОЯТ_СУПЕР_СИГУРЕН_КЛЮЧ_12345");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role) // 👈 Добавете този ред!
                 }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { user.Id, user.FirstName, role = user.Role, Token = tokenString});
            //return Ok(new
            //{
            //    Token = token,
            //    firstName = user.FirstName, // 🔥 Вече имаме FirstName
            //    role = user.Role // 🔥 Добавяме Role
            //});
        }

        // Клас за login заявката
        //public class LoginRequest
        //{
        //    public string? Email { get; set; }
        //    public string? Password { get; set; }
        //}


        [Authorize]
        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound("Потребителят не съществува.");

            user.FirstName = updatedUser.FirstName;
            user.Email = updatedUser.Email;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/user/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound("Потребителят не съществува.");

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Потребителят е изтрит успешно" });
            //return Ok("Потребителят е изтрит.");
        }

        ////test decode JWT ?
        //// GET /decode-token?token=ТОКЕНЪТ_ТУК
        //[HttpGet("decode-token")]
        //public IActionResult DecodeToken(string token)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    var jwtToken = handler.ReadJwtToken(token);

        //    return Ok(jwtToken.Claims.Select(c => new { c.Type, c.Value }));
        //}
    }
}
