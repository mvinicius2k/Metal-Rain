using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSimulationGroup))]
public partial struct BulletSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

        state.RequireForUpdate<Bullet>();


    }

    public void OnUpdate(ref SystemState state)
    {
        //var bulletEcb = new EntityCommandBuffer(Allocator.TempJob);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var physicsEcb = new EntityCommandBuffer(Allocator.TempJob);
        var bulletJob = new BulletJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            Speed = 15f,
            DefenseLookup = state.GetComponentLookup<TankDefense>(),
            GlobalTransformLookup = state.GetComponentLookup<LocalToWorld>(),
            ParentLookup = state.GetComponentLookup<Parent>(),
            Physics = physicsWorld,
            Ecb = physicsEcb.AsParallelWriter(),


        }.ScheduleParallel(state.Dependency);

        state.Dependency = bulletJob;

        state.Dependency.Complete();
        physicsEcb.Playback(state.EntityManager);
        physicsEcb.Dispose();


    }
}

[BurstCompile]
public partial struct BulletJob : IJobEntity
{
    public float Speed, DeltaTime;
    [ReadOnly]
    public PhysicsWorld Physics;
    [ReadOnly]
    public ComponentLookup<Parent> ParentLookup;
    [ReadOnly]
    public ComponentLookup<LocalToWorld> GlobalTransformLookup;
    [ReadOnly]
    public ComponentLookup<TankDefense> DefenseLookup;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute(BulletAspect bulletAspect, [ChunkIndexInQuery] int sortkey)
    {
        var globalTransform = GlobalTransformLookup.GetRefRO(bulletAspect.Entity);
        var hits = new NativeList<DistanceHit>(Allocator.Temp);
        
        var center = GlobalTransformLookup.GetRefRO(bulletAspect.Bullet.ValueRO.Center).ValueRO.Position;
        var bulletRotation = globalTransform.ValueRO.Rotation;

        var sucess = Physics.OverlapBox(center, bulletRotation, bulletAspect.ColliderSize, ref hits, bulletAspect.Bullet.ValueRO.Layer);

        if (sucess)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (ParentLookup.TryGetComponent(hits[i].Entity, out var parent)
                    && DefenseLookup.HasComponent(parent.Value))
                {
                    Ecb.AppendToBuffer(sortkey, parent.Value, new Damage
                    {
                        Value = bulletAspect.Bullet.ValueRO.Damage
                    });
                    //Debug.Log($"Bala {bulletAspect.Entity} atingiu {parent}");
                    Ecb.DestroyEntity(sortkey, bulletAspect.Entity);
                }
            }
        }
        else
        {
            var translationValue = Speed * DeltaTime * math.normalize(globalTransform.ValueRO.Forward);
            bulletAspect.LocalTransform.ValueRW.Position += translationValue;
            //var newPosition = bulletAspect.LocalTransform.ValueRO.Translate(translationValue).Position;
            //bulletAspect.LocalTransform.ValueRW.Position = newPosition;
            bulletAspect.Countdown.ValueRW.Value -= DeltaTime;

            if(bulletAspect.Countdown.ValueRO.Value <= 0f)
                Ecb.DestroyEntity(sortkey, bulletAspect.Entity);
        }




    }
}
