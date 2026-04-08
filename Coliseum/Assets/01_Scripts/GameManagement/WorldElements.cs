using System.Collections.Generic;
using Unity.Netcode;

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

public struct ElementDamage : INetworkSerializable
{
    public WorldElements Element;
    public float Percentage;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter // traductor a bytes para enviarlo por la red
    {
        serializer.SerializeValue(ref Element);
        serializer.SerializeValue(ref Percentage);
    }
}
// Crear elemento
// ElementDamage fire = new ElementDamage { Element = WorldElements.Fire, Percentage = 0.5f };