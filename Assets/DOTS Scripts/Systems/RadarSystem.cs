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
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {


            Debug.Log("Mirando");
            //var redTanks = SystemAPI.Query<TankAspect, RedTeamTag>();

            //var greenTanks = SystemAPI.QueryBuilder().WithAll<TankAspect, GreenTeamTag>().Build().ToEntityArray(Allocator.Temp);
            //NativeList<float3> redPositions = new NativeList<float3>(Allocator.TempJob);
            var query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TankProperties, LocalToWorld, RedTeamTag>()
                .Build(ref state);
            var redPositions = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            var redTanks = query.ToEntityArray(Allocator.TempJob);

            var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            new TankAimJob
            {
                RedTanksPositions = new NativeArray<LocalToWorld>(redPositions, Allocator.TempJob),
                RedTanks = new NativeArray<Entity>(redTanks, Allocator.TempJob),
                Ecb = ecb.AsParallelWriter(),
                Random = new Unity.Mathematics.Random(50)
                

            }.ScheduleParallel();

            
           
        }


    }
}

[BurstCompile]
public partial struct TankAimJob : IJobEntity
{
    //public QueryEnumerable<TankAspect, GreenTeamTag> GreenTanks;//
    [ReadOnly]
    public NativeArray<LocalToWorld> RedTanksPositions;
    [ReadOnly]
    public NativeArray<Entity> RedTanks;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public Unity.Mathematics.Random Random;
    [BurstCompile]
    public void Execute(TankAspect tank, GreenTeamTag tag, [ChunkIndexInQuery]int sortkey)
    {
        if (!tank.AimLocked)
        {
            var selectedIndex = Random.NextInt(0, RedTanksPositions.Length);

            var tankSelected = RedTanksPositions[selectedIndex];

            var target = tankSelected.Position;
            Debug.Log($"Setando mira para {target}");
            var yAngle = tank.GetYRotation(target);
            var newTransform = new LocalTransform
            {
                Position = tank.LocalTransform.ValueRO.Position,
                Scale = 1f,
                Rotation = quaternion.EulerXYZ(0f, yAngle, 0f)
            };
            Ecb.SetComponent(sortkey, tank.Entity, newTransform);
            
            var buffer = Ecb.AddBuffer<ApplyDamage>(sortkey, RedTanks[selectedIndex]);
            buffer.Add(new ApplyDamage
            {
                From = tank.Entity,
                Cadence = tank.Properties.ValueRO.Blob.Value.Cadence,
                Damage = tank.Properties.ValueRO.Blob.Value.Damage,
            });

            var rechargeDuration = tank.Properties.ValueRO.Blob.Value.Delay;
            Ecb.AddComponent(sortkey, RedTanks[selectedIndex], new ApplyDamageTimer
            {
                Value = Random.NextFloat(0f, rechargeDuration)
            }) ;
            
            tank.AimLocked = true;

        }
    }
}

