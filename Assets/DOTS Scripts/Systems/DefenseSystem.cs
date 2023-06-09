using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateAfter(typeof(AttackSystem)), UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DefenseSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Damage>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

        new DigestDamageJob
        {
            Ecb = ecb,

        }.Run();


        //state.Dependency.Complete();
        //ecb.Playback(state.EntityManager);
        //ecb.Dispose();
    }
}

//[BurstCompile]
public partial struct DigestDamageJob : IJobEntity
{
    public EntityCommandBuffer Ecb;
   // [BurstCompile]
    public void Execute(AttackedTankAspect attackedTank)
    {
        for (int i = 0; i < attackedTank.DamageBuffer.Length; i++)
        {
            attackedTank.Life -= attackedTank.DamageBuffer[i].Value;
        }

        //Se morrer
        if (attackedTank.Life <= 0)
        {

            //Debug.Log($"Matando {attackedTank.Entity}");

            //Seta tags para definir como morto e para limpar tank morto
            Ecb.SetComponentEnabled<AliveTankTag>(attackedTank.Entity, false);
            Ecb.AddComponent<TankCleanup>(attackedTank.Entity);
            Ecb.DestroyEntity(attackedTank.Entity);
            //Debug.Log($"Tank destruído {attackedTank.Entity}");
            
        }


        attackedTank.DamageBuffer.Clear();


    }
}
