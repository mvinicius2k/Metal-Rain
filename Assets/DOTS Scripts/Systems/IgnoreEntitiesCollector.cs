using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using RaycastHit = Unity.Physics.RaycastHit;

/// <summary>
/// Collector para ignorar certas entidades.
/// </summary>
public struct IgnoreEntitiesCollector : ICollector<RaycastHit>
{
    public NativeArray<Entity> Ignore;
    public bool hitted;
    public bool EarlyOutOnFirstHit => false;

    public RaycastHit ClosestHit;
    public float MaxFraction { get; private set; }

    public int NumHits { get; private set; }

    public IgnoreEntitiesCollector(Entity ignore)
    {
        Ignore = new NativeArray<Entity>(1, Allocator.Temp);
        Ignore[0] = ignore;//
        ClosestHit = default;
        MaxFraction = 1f;
        NumHits = 0;
        hitted = false;
    }

    /// <summary>
    /// Chamado toda vez que um hit acontece
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    public bool AddHit(RaycastHit hit)
    {
        
        if (!Ignore.Contains(hit.Entity))
        {
            //Vendo se o novo hit é mais perto
            if (!hitted || hit.Fraction < ClosestHit.Fraction)
                ClosestHit = hit;
            NumHits++;
            hitted = true;
            return true;
        }
        else
            return false;
    }
}
