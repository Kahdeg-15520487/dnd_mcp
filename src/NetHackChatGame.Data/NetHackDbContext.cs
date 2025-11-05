using Microsoft.EntityFrameworkCore;
using NetHackChatGame.Data.Entities;

namespace NetHackChatGame.Data;

/// <summary>
/// Database context for NetHack Chat Game
/// </summary>
public class NetHackDbContext : DbContext
{
    public NetHackDbContext(DbContextOptions<NetHackDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ConversationEntity> Conversations { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<GameSnapshotEntity> GameSnapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserEntity
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.LastLoginAt)
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure ConversationEntity
        modelBuilder.Entity<ConversationEntity>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PlayerName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.LastMessageAt)
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Conversations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.IsActive)
                .HasFilter("is_active = true");
        });

        // Configure MessageEntity
        modelBuilder.Entity<MessageEntity>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.Property(e => e.Content)
                .IsRequired();
                
            entity.Property(e => e.ToolCalls)
                .HasColumnType("jsonb");
                
            entity.Property(e => e.ToolResults)
                .HasColumnType("jsonb");
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ConversationId, e.SequenceNumber })
                .IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Role);
        });

        // Configure GameSnapshotEntity
        modelBuilder.Entity<GameSnapshotEntity>(entity =>
        {
            entity.ToTable("game_snapshots");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.GameStateJson)
                .IsRequired()
                .HasColumnType("jsonb");
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Conversation)
                .WithMany(e => e.GameSnapshots)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Message)
                .WithMany(e => e.GameSnapshots)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MessageId)
                .IsUnique();
            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt });
        });
    }
}
