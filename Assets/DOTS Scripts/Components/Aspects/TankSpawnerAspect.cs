using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct TankSpawnerAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRO<TankSpawner> Spawner;
    public readonly RefRO<LocalTransform> LocalTransform;
    public readonly DynamicBuffer<TankSpawnerRateBuffer> TankRates;
    public readonly RefRW<RandomGenerator> Random;

    /// <summary>
    /// Obtém um número referente a se as dimensões de spawn estão invertidas (negativas)
    /// </summary>
    public (float x, float y) Coeficients
    {
        get
        {
            var x = Spawner.ValueRO.End.x >= Spawner.ValueRO.Start.x ? 1f : -1f;
            var y = Spawner.ValueRO.End.y >= Spawner.ValueRO.Start.y ? 1f : -1f;
            return (x, y);
        }

    }
    public int MaxXTanks
        => (int)math.trunc(math.abs(Spawner.ValueRO.Start.x - Spawner.ValueRO.End.x) / Spawner.ValueRO.BlockSize.x);

    public int MaxZTanks
        => (int)math.trunc(math.abs(Spawner.ValueRO.Start.y - Spawner.ValueRO.End.y) / Spawner.ValueRO.BlockSize.y);

    public float3 TankSpawnEuler => new float3(0f, Spawner.ValueRO.Orientation, 0f);

}


