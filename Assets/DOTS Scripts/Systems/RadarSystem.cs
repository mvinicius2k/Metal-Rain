using System;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;

[BurstCompile]
public partial struct RadarSystem : ISystem
{
    private bool started;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankProperties>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetKeyDown(KeyCode.Space))
            started = true;

        if (!started)
            return;



        var redTanksQuery = new EntityQueryBuilder(Allocator.Temp)
                 .WithAll<TankProperties, LocalToWorld, RedTeamTag, AliveTankTag>()
                 .Build(ref state);
        var greenTanksQuery = new EntityQueryBuilder(Allocator.Temp)
             .WithAll<TankProperties, LocalToWorld, GreenTeamTag, AliveTankTag>()
             .Build(ref state);

        var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

        new TankAimJob
        {
            RedTanksPositions = redTanksQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
            RedTanks = redTanksQuery.ToEntityArray(Allocator.TempJob),
            GreenTanks = greenTanksQuery.ToEntityArray(Allocator.TempJob),
            GreenTanksPositions = greenTanksQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
            Ecb = ecb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),


        }.ScheduleParallel();
    }


}

[BurstCompile]
public partial struct TankAimJob : IJobEntity
{
    [ReadOnly]
    public NativeArray<LocalToWorld> RedTanksPositions, GreenTanksPositions;
    [ReadOnly]
    public NativeArray<Entity> RedTanks, GreenTanks;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public Unity.Mathematics.Random Random;
    [BurstCompile]//
    public void Execute(TankAspect tank, [ChunkIndexInQuery] int sortkey)
    {
        if (GreenTanks.Length == 0 || RedTanks.Length == 0)
            return;
        Debug.Log("Mirando");
        Entity selectedTank;
        LocalToWorld selectedTarget;
        if (tank.Team == Team.Red)
        {

            var randomIndex = Random.NextInt(0, GreenTanks.Length);
            selectedTarget = GreenTanksPositions[randomIndex];
            selectedTank = GreenTanks[randomIndex];
        }
        else
        {
            var randomIndex = Random.NextInt(0, RedTanks.Length);
            selectedTarget = RedTanksPositions[randomIndex];
            selectedTank = RedTanks[randomIndex];
        }

        tank.SetAimTo(selectedTarget.Position);
        Ecb.SetComponentEnabled<TankAimFreeTag>(sortkey, tank.Entity, false);

        var buffer = Ecb.AddBuffer<ApplyDamage>(sortkey, selectedTank);
        buffer.Add(new ApplyDamage
        {
            From = tank.Entity,
            Cadence = tank.Properties.ValueRO.Blob.Value.Cadence,
            Damage = tank.Properties.ValueRO.Blob.Value.Damage,
        });

        var rechargeDuration = tank.Properties.ValueRO.Blob.Value.Delay;
        Ecb.AddComponent(sortkey, selectedTank, new ApplyDamageTimer
        {
            Value = Random.NextFloat(0f, rechargeDuration)
        });

        //tank.AimLocked = true;

    }
}

