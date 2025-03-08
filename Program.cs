using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeBookApi.Models;
using Npgsql;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);



// 🔵 Port Configuration (работи и за Render, и локално)
var renderPort = Environment.GetEnvironmentVariable("RENDER_PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}"); // ⚠️ Важно за Render

// 🔵 CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => // 🚫 Не променяй името на политиката!
    {
        policy.WithOrigins(
            "http://localhost:4200",       // Локален Angular
            "https://вашият-frontend.vercel.app" // Добави тук Vercel домейна
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// 🔵 JWT Authentication (остава непроменена)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key", "❌ Липсва JWT ключ в конфигурацията!"))
            ),
            ValidateIssuer = false, // 🔴 Ако не използваш Issuer/Audience, остави false
            ValidateAudience = false
        };
    });


// 🔵 Database Configuration (без промени)
var usePostgreSQL = builder.Configuration.GetValue<bool>("UsePostgreSQL");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    try
    {
        if (usePostgreSQL)
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"));
        }
        else
        {
            options.UseMySql(
                builder.Configuration.GetConnectionString("MySQL"),
                new MySqlServerVersion(new Version(8, 0, 32))
            );
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Грешка при връзка с базата: {ex.Message}");
        throw;
    }
});


// Works on Local Добави JWT Authentication
// 🔹 Вземи connection string от appsettings.json
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

////Works on Render
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// 🔹 Регистрирай DbContext-а с MySQL

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 32))));

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        Console.WriteLine("git test");
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ТВОЯТ_СУПЕР_СИГУРЕН_КЛЮЧ_12345")), // Замени с реален ключ
//            ValidateIssuer = false,
//            ValidateAudience = false
//        };
//    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

// ❌ Махаме `app.UseHttpsRedirection();`
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Активирай контролерите
app.MapControllers();

app.Run();
