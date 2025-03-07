using Microsoft.EntityFrameworkCore;

namespace RecipeBookApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Добавяме таблицата "Users" от модела
        public DbSet<User> Users { get; set; }

        public DbSet<Recipe> user_recipes { get; set; } // ✅ Съвпада с MySQL
        //public DbSet<Recipe> Recipes { get; set; }

        //Това ще каже на Entity Framework (EF) да използва точно user_recipes, вместо UserRecipes.
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Recipe>().ToTable("user_recipes"); // 🔥 Синхронизираме името!
        //}
    }
}
