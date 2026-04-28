//  IngredientData.cs
//  Contains ALL enums and the PotionRecipe data class.
using Unity.Netcode;
using System.Collections.Generic;
using System;


// Every ingredient in the game. The first three are Bases, the rest are Modifiers.
// Enum backed by byte for low bandwidth
public enum IngredientType : byte 
{
    Cloud, Object, Puddle, // Bases
    Fire, Size, Float, Bouncy, Magnetic, Sparkle // Modifiers
}


// Immutable snapshot of what's inside a potion.
// Created by the Cauldron, stored on Potion, read by delivery mechanisms.
// Recipe as a Struct
public struct PotionRecipe : INetworkSerializable, IEquatable<PotionRecipe>
{
    public IngredientType Base;
    public bool HasBase; // Nullable types don't serialize well; use a bool flag instead
    
    // Fixed-size or predictable data is better for networking.
    // For simplicity, we'll store up to 3 modifiers.
    public IngredientType Mod1, Mod2, Mod3;
    public int ModifierCount;

    // I really cannot bother making this better. 
    public PotionRecipe(IngredientType? potionBase, List<IngredientType> modifiers)
    {
        if (potionBase != null)
        {
            Base = (IngredientType)potionBase;
            HasBase = true;
        }
        else
        {
            Base = IngredientType.Object;
            HasBase = false;
        }

        ModifierCount = modifiers.Count;

        Mod1 = modifiers.Count > 0 ? modifiers[0] : default;
        Mod2 = modifiers.Count > 1 ? modifiers[1] : default;
        Mod3 = modifiers.Count > 2 ? modifiers[2] : default;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Base);
        serializer.SerializeValue(ref HasBase);
        serializer.SerializeValue(ref Mod1);
        serializer.SerializeValue(ref Mod2);
        serializer.SerializeValue(ref Mod3);
        serializer.SerializeValue(ref ModifierCount);
    }

    public bool Equals(PotionRecipe other) => 
        Base == other.Base && HasBase == other.HasBase && ModifierCount == other.ModifierCount;

    public bool HasModifier(IngredientType mod) =>
        mod == Mod1 || mod == Mod2 || mod == Mod3;

}

// This wrapper makes the NetworkList happy
public struct IngredientNetworkElement : INetworkSerializable, IEquatable<IngredientNetworkElement>
{
    public IngredientType Type;

    // satisfies the IEquatable constraint
    public bool Equals(IngredientNetworkElement other) => Type == other.Type;

    // satisfies the network serialization requirement
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Type);
    }

    // This lets us treat IngredientNetworkElement like an IngredientType enum in our code
    // Thank you, Gemini
    public static implicit operator IngredientType(IngredientNetworkElement element) => element.Type;
    public static implicit operator IngredientNetworkElement(IngredientType type) => new() { Type = type };
}

// Quick category check so we don't scatter magic numbers everywhere.
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


/*
public class PotionRecipe
{
    // Null means "no base" → instant explosion delivery.
    public readonly IngredientType? Base;

    // 0-3 unique modifiers that stack.
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
*/