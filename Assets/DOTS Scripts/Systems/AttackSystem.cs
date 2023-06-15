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
        //var singleton = SystemAPI.GetSingleton<BeginVariableRateSimulationEntityCommandBufferSystem.Singleton>();
        //var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var arr = new NativeList<(TankProperties, Entity)>(Allocator.Temp);
        foreach (var (properties, entity) in SystemAPI.Query<TankProperties>().WithNone<Damage>().WithEntityAccess())
        {
            arr.Add((properties, entity));
        }


        //foreach (var aspect in SystemAPI.Query<ApplyDamageAspect>())
        //{
        //    if (aspect.Timer > 0)
        //    {
        //        aspect.Timer -= SystemAPI.Time.DeltaTime;
        //        break;
        //    }

        //    //if (Entities.Contains(aspect.TargetEntity))
        //    //{/
        //    //Debug.Log("Entidade tem buffer");
        //    //}

        //    Debug.Log($"Append em {aspect.TargetEntity}");
        //    if (!buffLook.HasBuffer(aspect.TargetEntity))
        //    {
        //        Debug.Log("Não tem");
        //    }

        //    ecb.AppendToBuffer(aspect.TargetEntity, new Damage
        //    {
        //        Value = aspect.BaseProperties.Damage,
        //    });

        //    //if(aspect.BaseProperties.Damage == 50f)
        //    //    Debug.Log($"Attack de {aspect.BaseProperties.Damage}");
        //    aspect.Timer = aspect.BaseProperties.Delay;
        //}

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


}

[BurstCompile]
public partial struct ApplyDamageJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer Ecb;
    public ComponentLookup<LocalToWorld> TransformLookup;
    [ReadOnly]
    public ComponentLookup<TankProperties> TankPropertiesLookup;
    //public NativeArray<Entity> Entities;
    public void Execute(ApplyDamageAspect aspect)
    {
        if (aspect.Timer > 0)
        {
            aspect.Timer -= DeltaTime;
            return;
        }


        //if (Entities.Contains(aspect.TargetEntity))
        //{/
        //Debug.Log("Entidade tem buffer");
        //}
        //if (!BufferLookup.HasBuffer(aspect.TargetEntity))
        //    Debug.Log("Vai dar rtuim");
        //Debug.Log($"{aspect.Entity} Append em {aspect.TargetEntity}");
        var bullet = Ecb.Instantiate(aspect.Properties.BulletPrefab);
        var firepointTransform = TransformLookup.GetRefRO(aspect.Properties.FirePoint);

        var enemyProperties = TankPropertiesLookup.GetRefRO(aspect.TargetEntity);
        var enemyTransform = TransformLookup.GetRefRO(enemyProperties.ValueRO.Center);

        //var enemyTransform = TransformLookup.GetRefRO(aspect.TargetEntity);
        var rot = TransformHelpers.LookAtRotation(firepointTransform.ValueRO.Position, enemyTransform.ValueRO.Position, math.up());

        //Debug.Log($"Lançando bala de {firepointTransform.ValueRO.Position}");
        //var eulerAngle = math.normalize(enemyTransform.ValueRO.Position - firepointTransform.ValueRO.Position);
        var comp = new LocalTransform
        {
            Position = firepointTransform.ValueRO.Position,
            Rotation = rot,
            Scale = 1f
        };

        Ecb.SetComponent(bullet, comp) ;

        //Ecb.AddComponent<Bullet>(bullet, new Bullet
        //{
        //    Entity = bullet,
        //    Damage = aspect.BaseProperties.Damage
        //});
        //Ecb.AppendToBuffer(aspect.TargetEntity, new Damage
        //{
        //    Value = aspect.BaseProperties.Damage,
        //});

        //if(aspect.BaseProperties.Damage == 50f)
        //    Debug.Log($"Attack de {aspect.BaseProperties.Damage}");
        aspect.Timer = aspect.BaseProperties.Delay;
    }
}
