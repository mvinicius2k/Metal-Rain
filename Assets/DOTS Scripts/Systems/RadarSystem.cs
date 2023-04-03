using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public partial struct RadarSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Mirando");
            //var redTanks = SystemAPI.Query<TankAspect, RedTeamTag>();

            //var greenTanks = SystemAPI.QueryBuilder().WithAll<TankAspect, GreenTeamTag>().Build().ToEntityArray(Allocator.Temp);
            NativeList<float3> redPositions = new NativeList<float3>(Allocator.TempJob);
            foreach(var tank in SystemAPI.Query<TankAspect,RedTeamTag>())
            {
                redPositions.Add(tank.Item1.LocalToWorld.ValueRO.Position);
            }

            
            new TankAimJob
            {
                RedTanksPositions = redPositions,

            }.ScheduleParallel();
            Debug.Log("Executado");
        }


    }
}

[BurstCompile]
public partial struct TankAimJob : IJobEntity
{
    //public QueryEnumerable<TankAspect, GreenTeamTag> GreenTanks;//
    public NativeList<float3> RedTanksPositions;
    public EntityCommandBuffer ecb;
    [BurstCompile]
    public void Execute(TankAspect tank, RedTeamTag tag)
    {
        if (!tank.AimLocked)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random();
            var selectedIndex = random.NextInt(0, RedTanksPositions.Length);
            var tankSelected = RedTanksPositions[selectedIndex];

            var target = tankSelected;

            tank.SetAimTo(target);
            tank.AimLocked = true;

        }
    }
}

