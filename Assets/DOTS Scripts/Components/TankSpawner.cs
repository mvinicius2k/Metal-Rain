using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct TankSpawner : IComponentData
{
    public float2 Start;
    public float2 End;
    public float2 BlockSize;
    public Entity ChosenTank;
    public Team Team;
}

public readonly partial struct TankSpawnerAspect : IAspect
{
    public readonly Entity Entity;
    private readonly RefRO<TankSpawner> spawner;
    public RefRO<TankSpawner> Spawner => spawner;
    public readonly RefRO< LocalTransform> LocalTransform;

    public int MaxXTanks 
        => (int) math.trunc(math.abs(Spawner.ValueRO.Start.x - Spawner.ValueRO.End.x) / Spawner.ValueRO.BlockSize.x);

    public int MaxZTanks
        => (int) math.trunc(math.abs(Spawner.ValueRO.Start.y - Spawner.ValueRO.End.y) / Spawner.ValueRO.BlockSize.y);
    
}


