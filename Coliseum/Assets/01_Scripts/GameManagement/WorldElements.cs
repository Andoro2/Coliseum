using System.Collections.Generic;

public enum WorldElements
{
    Null,
    Acid,
    Bludgeoning,
    Cold,
    Fire,
    Force,
    Lightning,
    Necrotic,
    Piercing,
    Poison,
    Psychic,
    Radiant,
    Slashing,
    Thunder
}

public struct ElementDamage
{
    public WorldElements Element;
    public float Percentage;
}
// Crear elemento
// ElementDamage fire = new ElementDamage { Element = WorldElements.Fire, Percentage = 0.5f };