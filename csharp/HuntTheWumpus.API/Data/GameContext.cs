using Microsoft.EntityFrameworkCore;
using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.API.Data;

public class GameContext : DbContext
{
    public GameContext(DbContextOptions<GameContext> options) : base(options)
    {
    }

    public DbSet<GameEntity> Games { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlayerId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CompletedAt);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.GameStateJson).HasColumnType("json");

            entity.HasIndex(e => e.PlayerId);
            entity.HasIndex(e => new { e.PlayerId, e.Status });
        });

        base.OnModelCreating(modelBuilder);
    }
}

// Simple entity for EF Core that doesn't have complex object dependencies
public class GameEntity
{
    public Guid Id { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public GameStatus Status { get; set; }
    public int Score { get; set; }
    public string GameStateJson { get; set; } = string.Empty;
}
