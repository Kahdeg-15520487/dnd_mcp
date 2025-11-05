namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents a monster in the game
/// </summary>
public class Monster
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Health { get; set; }
    public int Damage { get; set; }
    public int Defense { get; set; }
    public bool IsAlive { get; set; } = true;
    public int ExperienceReward { get; set; }
    public int GoldReward { get; set; }
    public List<Item> PossibleDrops { get; set; } = new();
}
