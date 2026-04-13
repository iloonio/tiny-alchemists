// ═══════════════════════════════════════════════════════════════
//  IngredientData.cs — Central data definitions for Alchemy
//  Contains ALL enums and the PotionRecipe data class.
//  No MonoBehaviour here — pure data.
// ═══════════════════════════════════════════════════════════════

using System.Collections.Generic;

/// <summary>
/// Every ingredient in the game. The first three are Bases, the rest are Modifiers.
/// </summary>
public enum IngredientType
{
    // ── Bases (max 1 per potion) ──
    Cloud,
    Object,     // spawns a Cube
    Puddle,

    // ── Modifiers (max 3 per potion, no duplicates) ──
    Fire,
    Size,
    Float,
    Bouncy,
    Magnetic,
    Sparkle
}

/// <summary>
/// Quick category check so we don't scatter magic numbers everywhere.
/// </summary>
public enum IngredientCategory { Base, Modifier }

public static class IngredientHelper
{
    public static IngredientCategory GetCategory(IngredientType type)
    {
        switch (type)
        {
            case IngredientType.Cloud:
            case IngredientType.Object:
            case IngredientType.Puddle:
                return IngredientCategory.Base;
            default:
                return IngredientCategory.Modifier;
        }
    }
}

/// <summary>
/// Immutable snapshot of what's inside a potion.
/// Created by the Cauldron, stored on Potion, read by delivery mechanisms.
/// </summary>
public class PotionRecipe
{
    /// <summary>Null means "no base" → instant explosion delivery.</summary>
    public readonly IngredientType? Base;

    /// <summary>0-3 unique modifiers that stack.</summary>
    public readonly List<IngredientType> Modifiers;

    public PotionRecipe(IngredientType? potionBase, List<IngredientType> modifiers)
    {
        Base = potionBase;
        Modifiers = modifiers ?? new List<IngredientType>();
    }

    public bool HasModifier(IngredientType mod) => Modifiers.Contains(mod);

    public override string ToString()
    {
        string b = Base.HasValue ? Base.Value.ToString() : "NoBase";
        string m = Modifiers.Count > 0 ? string.Join("+", Modifiers) : "NoMods";
        return $"[{b} | {m}]";
    }
}
