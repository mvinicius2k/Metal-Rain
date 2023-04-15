using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Services.Analytics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

[BurstCompile]
public partial struct TankSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankSpawner>();
    }
    public void OnUpdate(ref SystemState state)
    {

        if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.R)) //tecla G ou R spawna os tanks
        {
            var teamToSpawn = Team.Green;
            if (Input.GetKeyDown(KeyCode.R))
                teamToSpawn = Team.Red;


            var teamToSpawnStr = System.Enum.GetName(typeof(Team), teamToSpawn);
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            //var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);





            var tanksCount = new NativeReference<int>(Allocator.TempJob);
            new TankSpawnerPointsJob
            {
                Team = teamToSpawn,
                Ecb = ecb.AsParallelWriter(),
                MaxTanks = tanksCount //Para se obter a quantidade de tanques spawnados

            }.Schedule();


            if (AnaliticsSetup.Instance != null)
            {
                state.Dependency.Complete();
                stopwatch.Stop();

                var eventParams = new TankSpawnFieldModel
                {
                    DOTS = AnaliticsSetup.Instance.IsDOTS,
                    elapsedTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    tanksCount = tanksCount.Value,
                    team = teamToSpawnStr
                };

                AnalyticsService.Instance.CustomData(TankSpawnFieldModel.EventName, eventParams.GetEventParams());
                AnalyticsService.Instance.Flush();
                
                //ecb.Playback(state.EntityManager);
                //ecb.Dispose();

            }



        }
    }

}

[BurstCompile]
public partial struct TankSpawnerPointsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public Team Team;
    public NativeReference <int> MaxTanks;
    public void Execute([ChunkIndexInQuery] int sortKey, TankSpawnerAspect spawnerAspect)
    {
        if (spawnerAspect.Spawner.ValueRO.Team != Team)
            return;

        float2 BlockSize = spawnerAspect.Spawner.ValueRO.BlockSize;
        float2 Limits = new float2(spawnerAspect.MaxXTanks, spawnerAspect.MaxZTanks);

        MaxTanks.Value = spawnerAspect.MaxZTanks * spawnerAspect.MaxXTanks;

        var originFieldPosition = spawnerAspect.LocalTransform.ValueRO.Position;
        //Debug.Log($"Limits {Limits}");

        for (int count = 0; count < MaxTanks.Value; count++)
        {
            var zPosition = originFieldPosition.z + BlockSize.y * math.trunc(count / Limits.x);
            var xPosition = originFieldPosition.x + BlockSize.x * (count % Limits.x);

            var position = new float3(xPosition, 0, zPosition);
            var newTank = Ecb.Instantiate(sortKey, spawnerAspect.Spawner.ValueRO.ChosenTank);


            Ecb.SetComponent(sortKey, newTank, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = 1f

            });
            if (spawnerAspect.Spawner.ValueRO.Team == Team.Green)
                Ecb.AddComponent(sortKey, newTank, new GreenTeamTag());
            else
                Ecb.AddComponent(sortKey, newTank, new RedTeamTag());

        }

    }
}

