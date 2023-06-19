using Unity.Burst;
using Unity.Entities;

/// <summary>
/// Processa o dano recebido pelas balas e marca o tanque como morto quando a vida acaba
/// </summary>
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct DefenseSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Damage>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        var job = new DigestDamageJob
        {
#if MAINTHREAD
            SingleEcb = ecb,
#else
            Ecb = ecb.AsParallelWriter(),
#endif
        };

#if MAINTHREAD
        job.Run();
#else
        state.Dependency = job.Schedule(state.Dependency);
        state.Dependency.Complete();
#endif
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public partial struct DigestDamageJob : IJobEntity
    {
#if MAINTHREAD
        public EntityCommandBuffer SingleEcb;
#else
        public EntityCommandBuffer.ParallelWriter Ecb;
#endif
        //Itera sobre todas as defesas que tem dano a se processar
        [BurstCompile]
        public void Execute(AttackedTankAspect attackedTank, [ChunkIndexInQuery] int sortkey)
        {
            //Processando danos
            for (int i = 0; i < attackedTank.DamageBuffer.Length; i++)
            {
                attackedTank.Life -= attackedTank.DamageBuffer[i].Value;
            }

            //Se morrer
            if (attackedTank.Life <= 0)
            {
#if MAINTHREAD
                SingleEcb.SetComponentEnabled<AliveTankTag>( attackedTank.Entity, false);
                SingleEcb.AddComponent<TankCleanup>( attackedTank.Entity);
                SingleEcb.DestroyEntity( attackedTank.Entity);
#else
                Ecb.SetComponentEnabled<AliveTankTag>(sortkey, attackedTank.Entity, false);
                Ecb.AddComponent<TankCleanup>(sortkey, attackedTank.Entity);
                Ecb.DestroyEntity(sortkey, attackedTank.Entity);
#endif

            }

            //Limpando danos registrados
            attackedTank.DamageBuffer.Clear();


        }
    }
}
