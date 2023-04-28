using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public partial struct DefenseSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Damage>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        var digestJob = new DigestDamageJob
        {
            Ecb = ecb,

        };

        digestJob.Schedule();


        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial struct DigestDamageJob : IJobEntity
{
    public EntityCommandBuffer Ecb;
    public void Execute(AttackedTankAspect attackedTank)
    {
        for (int i = 0; i < attackedTank.DamageBuffer.Length; i++)
        {
            attackedTank.Life -= attackedTank.DamageBuffer[i].Value;
            Debug.Log($"Dano sofrido {attackedTank.DamageBuffer[i].Value} de { attackedTank.DamageBuffer[i].Source}");
        }


        //Se morrer
        if (attackedTank.Life <= 0)
        {
            for (int i = 0; i < attackedTank.DamageBuffer.Length; i++)
            {
                var tankToFree = attackedTank.DamageBuffer[i].Source;
                Ecb.SetComponent(tankToFree, new TankAttack
                {
                    Target = Entity.Null
                });
                Ecb.SetComponentEnabled<TankAttack>(tankToFree, false);
                Ecb.SetComponentEnabled<StandbyTankTag>(tankToFree, true);
            }
            Ecb.SetComponentEnabled<AliveTankTag>(attackedTank.Entity, false);
            Ecb.DestroyEntity(attackedTank.Entity);

        }

        attackedTank.DamageBuffer.Clear();


    }
}
