using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Services.Analytics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[BurstCompile, UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TankSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //Debug.unityLogger.logEnabled = false;
        state.RequireForUpdate<TankSpawner>();

    }
    public void OnUpdate(ref SystemState state)
    {

        if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.R)) //tecla G ou R spawna os tanks
        {
            var teamToSpawn = Team.Green;
            if (Input.GetKeyDown(KeyCode.R))
                teamToSpawn = Team.Red;


            var singleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

            new TankSpawnerPointsJob
            {
                Team = teamToSpawn,
                Ecb = ecb,


            }.Schedule();

            //new TankSpawnerPointsJob
            //{
            //    Team = teamToSpawn,
            //    Ecb = ecb.AsParallelWriter(),

            //}.ScheduleParallel();
        }
    }

}

[BurstCompile]
public partial struct TankSpawnerPointsJob : IJobEntity
{
    public EntityCommandBuffer Ecb;
    public Team Team;

    public int CalcWeightSum(in TankSpawnerAspect aspect)
    {
        var count = 0;
        for (int i = 0; i < aspect.TankRates.Length; i++)
        {
            count += aspect.TankRates[i].Weight;
        }
        return count;
    }

    public void Execute(TankSpawnerAspect spawn)
    {
        if (spawn.Spawner.ValueRO.Team != Team)
            return;

        var size = spawn.MaxXTanks * spawn.MaxZTanks;
        var fullWeight = CalcWeightSum(in spawn);
        var flatIdx = 0;
        var coeficients = spawn.Coeficients;
        var list = new NativeArray<Entity>(size, Allocator.Temp);
        var origin = spawn.LocalTransform.ValueRO.Position;
        var blockSize = spawn.Spawner.ValueRO.BlockSize;

        for (int i = 0; i < spawn.TankRates.Length; i++)
        {
            var total = spawn.TankRates[i].GetTotalFrom(size, fullWeight);
            for (int j = 0; j < total; j++)
            {
                list[flatIdx++] = spawn.TankRates[i].Prefab;
            }
        }

        spawn.Random.ValueRW.Value.Shuffle(ref list);

        flatIdx = 0;
        for (int i = 0; i < spawn.MaxXTanks; i++)
        {
            for (int z = 0; z < spawn.MaxZTanks; z++)
            {
                var prefab = list[flatIdx++];



                var newTank = Ecb.Instantiate(prefab);
                var position = new float3
                {
                    x = origin.x + i * blockSize.x * coeficients.x,
                    y = origin.y,
                    z = origin.z + z * blockSize.y * coeficients.y,
                };

                Ecb.SetComponent<LocalTransform>(newTank, new LocalTransform
                {
                    Position = position,
                    Rotation = quaternion.EulerXYZ(spawn.TankSpawnEuler),
                    Scale = 1f

                });

                if (spawn.Spawner.ValueRO.Team == Team.Green)
                    Ecb.AddComponent(newTank, new GreenTeamTag());
                else
                    Ecb.AddComponent(newTank, new RedTeamTag());
            }
        }
    }
}



//[BurstCompile]
//public partial struct TankSpawnerPointsJob : IJobEntity
//{
//    public EntityCommandBuffer.ParallelWriter Ecb;
//    public Team Team;
//    //public NativeReference <int> MaxTanks;
//    public void Execute(TankSpawnerAspect spawnerAspect, [ChunkIndexInQuery] int sortKey )
//    {
//        if (spawnerAspect.Spawner.ValueRO.Team != Team)
//            return;

//        float2 BlockSize = spawnerAspect.Spawner.ValueRO.BlockSize;
//        float2 Limits = new float2(spawnerAspect.MaxXTanks, spawnerAspect.MaxZTanks);

//        var maxTanks = spawnerAspect.MaxZTanks * spawnerAspect.MaxXTanks;

//        var originFieldPosition = spawnerAspect.LocalTransform.ValueRO.Position;
//        //Debug.Log($"Limits {Limits}");

//        for (int count = 0; count < maxTanks; count++)
//        {
//            var zPosition = originFieldPosition.z + BlockSize.y * math.trunc(count / Limits.x);
//            var xPosition = originFieldPosition.x + BlockSize.x * (count % Limits.x);

//            var position = new float3(xPosition, 0, zPosition);
//            var newTank = Ecb.Instantiate(sortKey, spawnerAspect.Spawner.ValueRO.ChosenTank);

//            Ecb.AddComponent<AliveTankTag>(sortKey, newTank);
//            Ecb.SetComponent(sortKey, newTank, new LocalTransform
//            {
//                Position = position,
//                Rotation = quaternion.EulerXYZ(spawnerAspect.TankSpawnEuler),
//                Scale = 1f

//            });




//            if (spawnerAspect.Spawner.ValueRO.Team == Team.Green)
//                Ecb.AddComponent(sortKey, newTank, new GreenTeamTag());
//            else
//                Ecb.AddComponent(sortKey, newTank, new RedTeamTag());

//        }

//    }
//}

