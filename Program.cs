using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeBookApi.Models;
using Npgsql;
using MySql.Data.MySqlClient; // Върни using за MySQL

var builder = WebApplication.CreateBuilder(args);

// 🔹 Фиксирай URL на API-то (локално)
//builder.WebHost.UseUrls("http://localhost:5000");
var renderPort = Environment.GetEnvironmentVariable("RENDER_PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");

// 🔹 Провери дали използваме PostgreSQL (за Render)
var usePostgreSQL = builder.Configuration.GetValue<bool>("UsePostgreSQL");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL на Angular приложението
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Ако използвате бисквитки/авторизация
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ТВОЯТ_СУПЕР_СИГУРЕН_КЛЮЧ_12345")), // Замени с реален ключ
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// 🔹 Конфигурирай базата данни
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (usePostgreSQL)
    {
        // PostgreSQL конфигурация за Render
        var pgConnection = builder.Configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string not found.");
        options.UseNpgsql(pgConnection);
    }
    else
    {
        // MySQL конфигурация за локална разработка
        var mySqlConnection = builder.Configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("MySQL connection string not found.");
        options.UseMySql(
            mySqlConnection,
            new MySqlServerVersion(new Version(8, 0, 32))
        );
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
