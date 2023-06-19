using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/// <summary>
/// Limpa tanques marcados como mortos 
/// </summary>
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CleanupTankSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankCleanup>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deadTanks = SystemAPI.QueryBuilder().WithAll<TankCleanup>().Build().ToEntityArray(Allocator.Temp);
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        //Libera todos os tanks que estão mirando no tank morto
        foreach (var (attack, entity) in SystemAPI.Query<TankAttack>().WithEntityAccess())
        {
            if (deadTanks.Contains(attack.Target))
            {
                ecb.SetComponentEnabled<StandbyTankTag>(entity, true);
                ecb.SetComponentEnabled<TankAttack>(entity, false);
            }
        }

       
        //Por fim, manda os tanks mortos pro limbo
        for (int i = deadTanks.Length - 1; i >= 0; i--)
            ecb.RemoveComponent<TankCleanup>(deadTanks[i]);

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

    }
}
