using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RecipeBookApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration config, AppDbContext dbContext, ILogger<AuthController> logger)
    {
        _config = config;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == userDto.Email && u.PasswordHash == userDto.Password);

        if (user == null)
        {
            _logger.LogWarning($"Login failed for email: {userDto.Email}");
            return Unauthorized("Невалидни креденшъли.");
        }

        var token = GenerateJwtToken(user.Email);
        return Ok(new { token });
    }

    private string GenerateJwtToken(string email)
    {
        var user = _dbContext.Users
            .Where(u => u.Email == email)
            .Select(u => new { u.Id, u.Email, u.Role })
            .FirstOrDefault();

        if (user == null)
        {
            _logger.LogError($"User with email {email} not found during JWT generation.");
            return string.Empty;
        }

        var role = user.Role ?? "user";
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        _logger.LogInformation($"Generated JWT for {email} with role {role}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}