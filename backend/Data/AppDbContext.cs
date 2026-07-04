using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Text.Json;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        :base(options)
    {
    }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
	public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Item>()
            .HasKey(i => i.Name);

		modelBuilder.Entity<Recipe>()
     		.Property(r => r.Ingredients)
     		.HasConversion(
         		v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
         		v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, string>()
     		);

        modelBuilder.Entity<Item>().HasData(
            new Item { Name = "stick", LocalizationName = "Палиця" },
            new Item { Name = "cobblestone", LocalizationName = "Камінь" },
            new Item { Name = "stone_sword", LocalizationName = "Кам'яний меч" }
        );
    
        modelBuilder.Entity<Recipe>().HasData(
            new
            {
                Id = 1,
                Name = "stone_sword_1",
                ItemResult = "stone_sword",
                Count = 1,
                IsShapeless = false,
                Ingredients = new Dictionary<string, string>
                {
                    { "2", "cobblestone" },
             		{ "5", "cobblestone" },
             		{ "8", "stick" }
                }
            }
        );
    }
}
