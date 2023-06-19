using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Spawna os tanque vermelhos e verdes de cada vez
/// </summary>

[BurstCompile, UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TankSpawnerSystem : ISystem
{
    //Usado para os tanque não atirarem todos ao mesmo tempo
    public const float RadarRandomCeil = 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankSpawner>();

    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.R)) //tecla G ou R spawna os tanks
        {
            var teamToSpawn = Team.Green;
            if (Input.GetKeyDown(KeyCode.R))
                teamToSpawn = Team.Red;


            var singleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

            var job = new TankSpawnerPointsJob
            {
                Team = teamToSpawn,
#if MAINTHREAD
                SingleEcb = ecb,
#else
                Ecb = ecb.AsParallelWriter(),
#endif



            };

#if MAINTHREAD
            job.Run();
#else
            job.Schedule();
#endif
        }
    }

}

[BurstCompile]
public partial struct TankSpawnerPointsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public EntityCommandBuffer SingleEcb;
    public Team Team;

    [BurstCompile]
    public int CalcWeightSum(in TankSpawnerAspect aspect)
    {
        var count = 0;
        for (int i = 0; i < aspect.TankRates.Length; i++)
        {
            count += aspect.TankRates[i].Weight;
        }
        return count;
    }
    [BurstCompile]
    public void Execute(TankSpawnerAspect spawn, [ChunkIndexInQuery] int sortkey)
    {
        if (spawn.Spawner.ValueRO.Team != Team)
            return;

        //Número maximo de tanques
        var size = spawn.MaxXTanks * spawn.MaxZTanks;
        //Peso total de todos os tipos de tanques
        var fullWeight = CalcWeightSum(in spawn);

        var coeficients = spawn.Coeficients;

        //lista de todos os tanques a se spawnar
        var list = new NativeList<Entity>(size, Allocator.Temp);
        var origin = spawn.LocalTransform.ValueRO.Position;
        var blockSize = spawn.Spawner.ValueRO.BlockSize;
        for (int i = 0; i < spawn.TankRates.Length; i++)
        {
            //Quantidade total de tanques de um certo tipo
            var total = spawn.TankRates[i].GetTotalFrom(size, fullWeight);
            for (int j = 0; j < total; j++)
            {
                //Adicioanndo tanque do tipo
                list.Add(spawn.TankRates[i].Prefab);
                if (list.Length == list.Capacity) //Quantidade de tanques atingido
                    goto RandomizeList; //Saindo dos laços

            }
        }

        //Randomizando a posição dos tanques
    RandomizeList:
        spawn.Random.ValueRW.Value.Shuffle(ref list);

        var flatIdx = 0;
        for (int i = 0; i < spawn.MaxXTanks; i++)
        {
            for (int z = 0; z < spawn.MaxZTanks; z++)
            {
                var prefab = list[flatIdx++];


#if MAINTHREAD
                var newTank = SingleEcb.Instantiate(prefab);
#else
                var newTank = Ecb.Instantiate(sortkey, prefab);
#endif
                var position = new float3
                {
                    x = origin.x + i * blockSize.x * coeficients.x,
                    y = origin.y,
                    z = origin.z + z * blockSize.y * coeficients.y,
                };

#if MAINTHREAD
                SingleEcb.SetComponent<LocalTransform>(newTank, new LocalTransform
#else
                Ecb.SetComponent<LocalTransform>(sortkey, newTank, new LocalTransform
#endif
                {
                    Position = position,
                    Rotation = quaternion.EulerXYZ(spawn.TankSpawnEuler),
                    Scale = 1f

                });

                //Necessário para todos os tanque não atirem ao mesmo tempo nos testes. Possivelmente dispensável no jogo real
#if MAINTHREAD
                SingleEcb.SetComponent(newTank, new TankAttack
#else
                Ecb.SetComponent(sortkey, newTank, new TankAttack
#endif
                {
                    RadarTimer = spawn.Random.ValueRW.Value.NextFloat(0f, TankSpawnerSystem.RadarRandomCeil)
                }) ;

                if (spawn.Spawner.ValueRO.Team == Team.Green)
                {

#if MAINTHREAD
                    SingleEcb.AddComponent(newTank, new GreenTeamTag());
#else
                    Ecb.AddComponent(sortkey, newTank, new GreenTeamTag());
#endif
                }
                else
                {
#if MAINTHREAD
                    SingleEcb.AddComponent(newTank, new RedTeamTag());
#else
                    Ecb.AddComponent(sortkey, newTank, new RedTeamTag());
#endif
                }

            }
        }
    }
}

