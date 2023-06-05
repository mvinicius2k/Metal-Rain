using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct TankSpawnerAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRO<TankSpawner> Spawner;
    public readonly RefRO< LocalTransform> LocalTransform;

    public int MaxXTanks 
        => (int) math.trunc(math.abs(Spawner.ValueRO.Start.x - Spawner.ValueRO.End.x) / Spawner.ValueRO.BlockSize.x);

    public int MaxZTanks
        => (int) math.trunc(math.abs(Spawner.ValueRO.Start.y - Spawner.ValueRO.End.y) / Spawner.ValueRO.BlockSize.y);
    public float3 TankSpawnEuler => new float3(0f, Spawner.ValueRO.Orientation, 0f);
    
}


