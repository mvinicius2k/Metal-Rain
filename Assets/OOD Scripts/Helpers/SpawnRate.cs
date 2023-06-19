using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SpawnRate
{
    public int Weight;
    public TankKind Kind;
    public GameObject Prefab;

    public int GetTotalFrom(float size, float totalWeight)
    {
        var rate = Weight / totalWeight;
        return (int)math.ceil(size * rate);
    }
}
