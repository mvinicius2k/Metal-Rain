using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CleanupTankSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankCleanup>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var deadTanks = SystemAPI.QueryBuilder().WithAll<TankCleanup>().Build().ToEntityArray(Allocator.Temp);
        var singleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);


        //Libera todos os tanks que estão mirando no tank morto
        foreach (var allAttacked in SystemAPI.Query<AttackedTankAspect>())
        {
            if (deadTanks.Contains(allAttacked.Attack.ValueRO.Target))
            {
                ecb.SetComponentEnabled<StandbyTankTag>(allAttacked.Entity, true);
                ecb.SetComponentEnabled<TankAttack>(allAttacked.Entity, false);
            }
        }

        //Por fim, manda os tanks mortos pro limbo
        for (int i = deadTanks.Length - 1; i >= 0; i--)
        {
            ecb.RemoveComponent<TankCleanup>(deadTanks[i]);
        }

    }
}
