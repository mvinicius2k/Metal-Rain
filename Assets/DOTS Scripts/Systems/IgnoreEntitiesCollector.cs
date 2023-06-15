using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using static UnityEngine.UI.Image;
using RaycastHit = Unity.Physics.RaycastHit;

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

    public bool AddHit(RaycastHit hit)
    {

        if (!Ignore.Contains(hit.Entity))
        {
            if (!hitted || hit.Fraction < ClosestHit.Fraction)
                ClosestHit = hit;

            hitted = true;
            return true;
        }
        else
            return false;
    }
}
