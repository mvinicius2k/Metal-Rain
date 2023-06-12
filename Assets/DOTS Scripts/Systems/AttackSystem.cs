using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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


        var buffLook = state.GetBufferLookup<Damage>();
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

        new ApplyDamageJob
        {
            Ecb = ecb,
            DeltaTime = SystemAPI.Time.DeltaTime,
            BufferLookup = buffLook,

        }.Run();


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
    public BufferLookup<Damage> BufferLookup;
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

            Ecb.AppendToBuffer(aspect.TargetEntity, new Damage
            {
                Value = aspect.BaseProperties.Damage,
            });

        //if(aspect.BaseProperties.Damage == 50f)
        //    Debug.Log($"Attack de {aspect.BaseProperties.Damage}");
        aspect.Timer = aspect.BaseProperties.Delay;
    }
}
