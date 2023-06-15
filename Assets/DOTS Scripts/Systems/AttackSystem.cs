using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(RadarSystem)), UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
public partial struct AttackSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankAttack>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var arr = new NativeList<(TankProperties, Entity)>(Allocator.Temp);
        foreach (var (properties, entity) in SystemAPI.Query<TankProperties>().WithNone<Damage>().WithEntityAccess())
        {
            arr.Add((properties, entity));
        }

        var job = new ApplyDamageJob
        {
            Ecb = ecb,
            DeltaTime = SystemAPI.Time.DeltaTime,
            TransformLookup = state.GetComponentLookup<LocalToWorld>(),
            TankPropertiesLookup = state.GetComponentLookup<TankProperties>()


        }.Schedule(state.Dependency);

        state.Dependency = job;

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public partial struct ApplyDamageJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer Ecb;
        public ComponentLookup<LocalToWorld> TransformLookup;
        [ReadOnly]
        public ComponentLookup<TankProperties> TankPropertiesLookup;
        public void Execute(ApplyDamageAspect aspect)
        {
            if (aspect.Timer > 0)
            {
                aspect.Timer -= DeltaTime;
                return;
            }

            var bullet = Ecb.Instantiate(aspect.Properties.BulletPrefab);
            var firepointTransform = TransformLookup.GetRefRO(aspect.Properties.FirePoint);

            var enemyProperties = TankPropertiesLookup.GetRefRO(aspect.TargetEntity);
            var enemyTransform = TransformLookup.GetRefRO(enemyProperties.ValueRO.Center);

            var rot = TransformHelpers.LookAtRotation(firepointTransform.ValueRO.Position, enemyTransform.ValueRO.Position, math.up());

            var comp = new LocalTransform
            {
                Position = firepointTransform.ValueRO.Position,
                Rotation = rot,
                Scale = 1f
            };

            Ecb.SetComponent(bullet, comp);

            aspect.Timer = aspect.BaseProperties.Delay;
        }
    }
}