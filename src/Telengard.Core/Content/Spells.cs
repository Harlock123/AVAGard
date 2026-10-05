namespace Telengard.Core.Content;

public sealed record SpellInfo(int Number, int Level, int Index, string Name, bool Combat, string Description);

/// <summary>
/// The 36 spells (6 levels x 6). Names follow the manual's spell card; effects follow V4.18
/// (see docs/research/c64-mechanics.md §5). Descriptions are newly written for this project.
/// </summary>
public static class Spells
{
    public static readonly IReadOnlyList<SpellInfo> All =
    [
        S(1, "Magic Missile", true, "Strikes the monster for 5-12 damage."),
        S(2, "Sleep", true, "Puts a living monster to sleep (resisted by low Intelligence; undead are immune). A sleeping monster can usually be slain."),
        S(3, "Cure Light Wounds", false, "Restores 1-8 hit points."),
        S(4, "Light", false, "For 5-15 turns, reveals features in neighbouring rooms and scares off some undead."),
        S(5, "Turn Undead", true, "May drive off an undead monster (Wisdom and level vs monster level). You gain experience."),
        S(6, "Protection from Evil", false, "For 5-15 turns, prevents level drain and hampers specters, vampires and demons."),
        S(7, "Web", true, "Entangles the monster (resisted by stronger monsters); a webbed monster may be slain."),
        S(8, "Levitate", false, "For 5-25 turns, lets you float over pits."),
        S(9, "Cause Light Wounds", true, "Inflicts 3-14 damage."),
        S(10, "Detect Traps", false, "For 5-25 turns, usually warns you of trapped treasure."),
        S(11, "Charm", true, "Charms a living monster (Charisma); a charmed monster may be slain."),
        S(12, "Strength", false, "For 5-25 turns, +4 to hit and +5 damage."),
        S(13, "Lightning Bolt", true, "Inflicts 15 to 14+6×level damage."),
        S(14, "Cure Serious Wounds", false, "Restores 1-24 hit points."),
        S(15, "Continual Light", false, "As Light, for 5-35 turns."),
        S(16, "Invisibility", false, "For 5-25 turns, most monsters fail to find you."),
        S(17, "Hold Monster", true, "Holds the monster (Intelligence vs monster level); a held monster may be slain."),
        S(18, "Phantasmal Forces", true, "An illusion that may slay the monster outright (Intelligence vs monster level)."),
        S(19, "Pass Wall", false, "Step one room in any direction, through walls. Not usable in melee."),
        S(20, "Fireball", true, "Inflicts 15 to 14+12×level damage."),
        S(21, "Cause Serious Wounds", true, "Inflicts 10-41 damage."),
        S(22, "Flesh to Stone", true, "60% chance to turn the monster to stone."),
        S(23, "Fear", false, "For 5-34 turns, the four weakest monster types avoid you."),
        S(24, "Finger of Death", true, "May slay a living monster (better against low-level monsters)."),
        S(25, "Teleport", false, "Jump to a chosen offset (north, east, up); range depends on level. Not usable in melee."),
        S(26, "Astral Walk", false, "For 5-20 turns, walk through walls."),
        S(27, "Power Word Kill", true, "80% chance to slay a living monster."),
        S(28, "Ice Storm", true, "Inflicts 60 damage."),
        S(29, "Wall of Fire", true, "60% chance the monster leaves (no experience); otherwise it is burned for 8-19."),
        S(30, "Plague", true, "May slay a living monster, but can backfire fatally (Wisdom, Constitution)."),
        S(31, "Time Stop", false, "For 5-24 turns, most monsters cannot reach you. Ends a melee."),
        S(32, "Raise Dead", false, "For 5-44 turns, you may survive death (costs 1 Constitution; chance based on Constitution)."),
        S(33, "Holy Symbol", true, "90% chance to slay the monster."),
        S(34, "Word of Recall", false, "Returns you to the starting stairway, but you lose all items and carried gold. Not usable in melee."),
        S(35, "Restoration", false, "Restores all hit points."),
        S(36, "Prismatic Wall", true, "The monster is driven away (no experience)."),
    ];

    private static SpellInfo S(int n, string name, bool combat, string description) =>
        new(n, (n - 1) / 6 + 1, (n - 1) % 6 + 1, name, combat, description);

    public static SpellInfo Get(int level, int index) => All[(level - 1) * 6 + index - 1];

    /// <summary>Line 21002: spell level n is available when n &lt;= INT(LV/3)+1 (max 6).</summary>
    public static int MaxSpellLevel(int characterLevel) => Math.Min(6, characterLevel / 3 + 1);
}
