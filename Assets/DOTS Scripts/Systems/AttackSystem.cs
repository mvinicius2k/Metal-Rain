using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(RadarSystem))]
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
        if(aspect.Timer > 0)
        {
            aspect.Timer -= DeltaTime;
            return;
        }

        var buffer = Ecb.AddBuffer<Damage>(sortkey, aspect.TargetEntity);
        buffer.Add(new Damage
        {
            Value = aspect.BaseProperties.Damage,
            Source = aspect.Entity
        });
        

        aspect.Timer = aspect.BaseProperties.Delay;
    }
}
