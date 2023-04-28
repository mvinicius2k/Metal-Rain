using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

[RequireMatchingQueriesForUpdate]
[UpdateAfter(typeof(AttackSystem))]
public partial struct CleanDeadTanksSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        //var deadDamageSources = SystemAPI
        //    .QueryBuilder()
        //    .WithAll<CleanupTank>()
        //    .WithNone<AliveTankTag>()
        //    .Build()
        //    .ToEntityArray(Allocator.Temp);

        //if (deadDamageSources.Length == 0)
        //    return;
        //Debug.Log(deadDamageSources.Length);
        //var ecb = new EntityCommandBuffer(Allocator.Temp);
        //foreach (var (tag, damageBuffer, entity) in SystemAPI.Query<EnabledRefRO<AliveTankTag>, DynamicBuffer<ApplyDamage>>().WithEntityAccess())
        //{
        //    for (int i = damageBuffer.Length - 1; i >= 0; i--)
        //    {

        //        if (deadDamageSources.Contains(damageBuffer[i].From))
        //        {
        //            //ecb.SetBuffer<ApplyDamage>(entity).RemoveAt(i);
        //            damageBuffer.RemoveAt(i);
                    
        //        }

        //        ecb.RemoveComponent<CleanupTank>(deadDamageSources[i]);
        //    }

            
        //}

        

        ////var deadTanks = query.ToComponentDataArray<CleanupTank>(Allocator.Temp);
        ////for (int i = 0; i < deadTanks.Length; i++)
        ////{
        ////    for (int j = 0; j < deadTanks[i].TanksToFree.Length; j++)
        ////    {
        ////        if(deadTanks[i].TanksToFree[j] !=  Entity.Null)
        ////            ecb.SetComponentEnabled<TankAimFreeTag>(deadTanks[i].TanksToFree[j], true);
        ////        //if (state.EntityManager.HasComponent<AliveTankTag>(attacker)) ; //Se não estiver morrido ao mesmo tempo

        ////    }
        ////}

        //ecb.Playback(state.EntityManager);
        //ecb.Dispose();
    }
}
