using System;
using System.Linq;
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
        var buffLook = state.GetBufferLookup<Damage>();
        var deadTanks = SystemAPI.QueryBuilder().WithAll<TankCleanup>().Build().ToEntityArray(Allocator.Temp);
        //Debug.Log("------------------------------");
        for (int i = 0; i < deadTanks.Length; i++)
        {
            //Debug.Log($"{deadTanks[i]} está morto");
        }
        //var singleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        //var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        //Libera todos os tanks que estão mirando no tank morto
        foreach (var (attack, entity) in SystemAPI.Query<TankAttack>().WithEntityAccess())
        {
            if (deadTanks.Contains(attack.Target))
            {
                //Debug.Log($"{entity} não mirará mais em {attack.Target}");
                ecb.SetComponentEnabled<StandbyTankTag>(entity, true);
                ecb.SetComponentEnabled<TankAttack>(entity, false);
            }
            else
            {
                //Debug.Log($"{entity}, cujo alvo é o {attack.Target} está ok");
            }
        }

        foreach (var (tag, entity) in SystemAPI.Query<AliveTankTag>().WithEntityAccess())
        {
            //if (buffLook.HasBuffer(entity))
            //Debug.Log($"{entity} não tem ataque");
        }

        //Por fim, manda os tanks mortos pro limbo
        for (int i = deadTanks.Length - 1; i >= 0; i--)
        {
            //Debug.Log("Limpando tanque " + deadTanks[i]);
            ecb.RemoveComponent<TankCleanup>(deadTanks[i]);
        }

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

    }
}
//