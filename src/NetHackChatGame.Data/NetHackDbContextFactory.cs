using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetHackChatGame.Data;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations
/// </summary>
public class NetHackDbContextFactory : IDesignTimeDbContextFactory<NetHackDbContext>
{
    public NetHackDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NetHackDbContext>();
        
        // Use a default connection string for migrations
        // This will be overridden at runtime by the actual application configuration
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=nethack_chat_game;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly("NetHackChatGame.Data"));

        return new NetHackDbContext(optionsBuilder.Options);
    }
}
