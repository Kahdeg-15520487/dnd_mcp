namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents an item in the game
/// </summary>
public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool Equipped { get; set; }
    public int Quantity { get; set; } = 1;
    
    // Weapon/Armor specific
    public int DamageBonus { get; set; }
    public int DefenseBonus { get; set; }
    
    // Potion/Consumable specific
    public int HealthBonus { get; set; }
}
