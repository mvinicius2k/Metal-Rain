using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct DamageSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ApplyDamage>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        new ApplyDamageJob
        {
            Ecb = ecb.AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
        
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


}

public partial struct ApplyDamageJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public void Execute(ApplyDamageAspect aspect, [ChunkIndexInQuery] int sortkey)
    {

        for (int i = 0; i < aspect.ApplyDamage.Length; i++)
        {
            if (aspect.Timer.ValueRO.Value <= 0) //Hora de atirar
            {
                var damage = aspect.ApplyDamage[i].Damage;
                var rechargeDuration = aspect.Properties.ValueRO.Blob.Value.Delay;
                aspect.Properties.ValueRW.CurrentLife -= damage;
                aspect.Timer.ValueRW.Value = rechargeDuration;
                Debug.Log($"Novo {aspect.Properties.ValueRO.CurrentLife}");

                if (aspect.Properties.ValueRO.CurrentLife <= 0f)
                    Ecb.DestroyEntity(sortkey, aspect.Entity);
            }
            else
                aspect.Timer.ValueRW.Value -= DeltaTime;
            
            
        }
    }
}
