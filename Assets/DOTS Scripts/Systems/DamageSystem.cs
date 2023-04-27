using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(RadarSystem))]
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
        var damageSourcesCount = aspect.ApplyDamage.Length;

        for (int i = 0; i < damageSourcesCount; i++)
        {
            

            if (aspect.Timer <= 0) //Hora de atirar
            {
                var damage = aspect.ApplyDamage[i].Damage;
                
                aspect.Properties.ValueRW.CurrentLife -= damage;
                aspect.RestartTimer();
                Debug.Log($"Novo {aspect.Properties.ValueRO.CurrentLife}");

                if (aspect.Properties.ValueRO.CurrentLife <= 0f)
                {
                    var damageSources = new NativeArray<Entity>(damageSourcesCount, Allocator.Temp);
                    for (int d = 0; d < damageSourcesCount; d++)
                        Ecb.SetComponentEnabled<TankAimFreeTag>(sortkey,aspect.ApplyDamage[i].From, true);
                  
                    //Debug.Log($"Deletando {aspect.Entity} atacada pelos {damageSources}");
                    

                    Ecb.DestroyEntity(sortkey, aspect.Entity);
                    return;
                    
                }
            }
            else
                aspect.Timer -= DeltaTime;
            
            
        }
    }
}
