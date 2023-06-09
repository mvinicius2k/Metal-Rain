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
using Debug = UnityEngine.Debug;

[BurstCompile]
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


            //var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

            new TankSpawnerPointsJob
            {
                Team = teamToSpawn,
                Ecb = ecb.AsParallelWriter(),

            }.ScheduleParallel();
        }
    }

}



[BurstCompile]
public partial struct TankSpawnerPointsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public Team Team;
    //public NativeReference <int> MaxTanks;
    public void Execute(TankSpawnerAspect spawnerAspect, [ChunkIndexInQuery] int sortKey )
    {
        if (spawnerAspect.Spawner.ValueRO.Team != Team)
            return;

        float2 BlockSize = spawnerAspect.Spawner.ValueRO.BlockSize;
        float2 Limits = new float2(spawnerAspect.MaxXTanks, spawnerAspect.MaxZTanks);

        var maxTanks = spawnerAspect.MaxZTanks * spawnerAspect.MaxXTanks;

        var originFieldPosition = spawnerAspect.LocalTransform.ValueRO.Position;
        //Debug.Log($"Limits {Limits}");

        for (int count = 0; count < maxTanks; count++)
        {
            var zPosition = originFieldPosition.z + BlockSize.y * math.trunc(count / Limits.x);
            var xPosition = originFieldPosition.x + BlockSize.x * (count % Limits.x);

            var position = new float3(xPosition, 0, zPosition);
            var newTank = Ecb.Instantiate(sortKey, spawnerAspect.Spawner.ValueRO.ChosenTank);

            Ecb.AddComponent<AliveTankTag>(sortKey, newTank);
            Ecb.SetComponent(sortKey, newTank, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.EulerXYZ(spawnerAspect.TankSpawnEuler),
                Scale = 1f

            });

            


            if (spawnerAspect.Spawner.ValueRO.Team == Team.Green)
                Ecb.AddComponent(sortKey, newTank, new GreenTeamTag());
            else
                Ecb.AddComponent(sortKey, newTank, new RedTeamTag());

        }

    }
}

