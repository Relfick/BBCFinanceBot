using Microsoft.EntityFrameworkCore;

namespace BBCFinanceBot.db;

public class ApplicationContext: DbContext
{
    public DbSet<UserToken> UserTokens { get; set; } = null!;

    public ApplicationContext()
    {
        // Database.EnsureDeleted();
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=bot.db");
    }
}