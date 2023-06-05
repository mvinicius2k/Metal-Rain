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
            Ecb = ecb,
            DeltaTime = SystemAPI.Time.DeltaTime
        }.Schedule();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


}

public partial struct ApplyDamageJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer Ecb;
    public void Execute(ApplyDamageAspect aspect)
    {
        if (aspect.Timer > 0)
        {
            aspect.Timer -= DeltaTime;
            return;
        }

        
        
        Ecb.AppendToBuffer(aspect.TargetEntity, new Damage
        {
            Value = aspect.BaseProperties.Damage,
        });

        if(aspect.BaseProperties.Damage == 50f)
            Debug.Log($"Attack de {aspect.BaseProperties.Damage}");
        aspect.Timer = aspect.BaseProperties.Delay;
    }
}
