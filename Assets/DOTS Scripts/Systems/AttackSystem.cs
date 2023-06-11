using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
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
        var enemies = SystemAPI.QueryBuilder().WithAll<Damage>().Build().ToEntityArray(Allocator.TempJob);

        new ApplyDamageJob
        {
            Ecb = ecb,
            DeltaTime = SystemAPI.Time.DeltaTime,
            Entities = enemies
            

        }.Run();


        try
        {
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

        }
        catch (System.Exception e)
        {
            Debug.Log("Erro aqui");
            throw;
        }

    }


}

[BurstCompile]
public partial struct ApplyDamageJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer Ecb;
    public NativeArray<Entity> Entities;
    public void Execute(ApplyDamageAspect aspect)
    {
        if (aspect.Timer > 0)
        {
            aspect.Timer -= DeltaTime;
            return;
        }
        if(aspect.TargetEntity == Entity.Null)
        {
            Debug.Log("Entidade nula");
        }

        if (Entities.Contains(aspect.TargetEntity))
        {
            //Debug.Log("Entidade tem buffer");
        }

        Debug.Log($"Append em {aspect.TargetEntity}");

            Ecb.AppendToBuffer(aspect.TargetEntity, new Damage
            {
                Value = aspect.BaseProperties.Damage,
            });

        //if(aspect.BaseProperties.Damage == 50f)
        //    Debug.Log($"Attack de {aspect.BaseProperties.Damage}");
        aspect.Timer = aspect.BaseProperties.Delay;
    }
}
