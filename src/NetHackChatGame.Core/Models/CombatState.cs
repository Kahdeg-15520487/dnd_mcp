namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents the current state of a combat encounter
/// </summary>
public class CombatState
{
    public Monster Monster { get; set; } = null!;
    public int MonsterHealth { get; set; }
    public bool PlayerTurn { get; set; }
    public bool PlayerDefending { get; set; }
}
