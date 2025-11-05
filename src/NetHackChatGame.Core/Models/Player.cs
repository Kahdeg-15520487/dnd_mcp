namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents the player character
/// </summary>
public class Player
{
    public string Name { get; set; } = "Adventurer";
    public string Class { get; set; } = "Warrior";
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int Gold { get; set; } = 0;
    public List<Item> Inventory { get; set; } = new();
    public Guid? EquippedWeaponId { get; set; }
    public Guid? EquippedArmorId { get; set; }
    
    /// <summary>
    /// Get the currently equipped weapon
    /// </summary>
    public Item? GetEquippedWeapon() =>
        EquippedWeaponId.HasValue 
            ? Inventory.FirstOrDefault(i => i.Id == EquippedWeaponId.Value) 
            : null;
    
    /// <summary>
    /// Get the currently equipped armor
    /// </summary>
    public Item? GetEquippedArmor() =>
        EquippedArmorId.HasValue 
            ? Inventory.FirstOrDefault(i => i.Id == EquippedArmorId.Value) 
            : null;
    
    /// <summary>
    /// Calculate total weapon damage
    /// </summary>
    public int GetTotalDamage()
    {
        var weapon = GetEquippedWeapon();
        var baseDamage = weapon?.DamageBonus ?? 5;
        var levelBonus = Level * 2;
        return baseDamage + levelBonus;
    }
    
    /// <summary>
    /// Calculate total defense
    /// </summary>
    public int GetTotalDefense()
    {
        var armor = GetEquippedArmor();
        var baseDefense = armor?.DefenseBonus ?? 0;
        var levelBonus = Level;
        return baseDefense + levelBonus;
    }
}
