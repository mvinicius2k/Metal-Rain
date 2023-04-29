using System;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;

[BurstCompile]
public partial struct RadarSystem : ISystem
{
    private bool started;
    private bool endgame;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TankProperties>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (endgame)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            started = true;

        if (!started)
            return;


        //Todos os tanks do time vermelho vivos
        var redTanksQuery = new EntityQueryBuilder(Allocator.Temp)
                 .WithAll<TankProperties, LocalToWorld, RedTeamTag, AliveTankTag>()
                 .Build(ref state);
        //Verdes também
        var greenTanksQuery = new EntityQueryBuilder(Allocator.Temp)
             .WithAll<TankProperties, LocalToWorld, GreenTeamTag, AliveTankTag>()
             .Build(ref state);

        //Se não houver nenhum tanque em algum dos lados, gameover
        if(redTanksQuery.IsEmpty || greenTanksQuery.IsEmpty)
        {
            endgame = true;
            Debug.Log("Fim de jogo");
            return;
        }
        //var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        //var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);
        //Usar sync points

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        new TankAimJob
        {
            RedTanksPositions = redTanksQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
            RedTanks = redTanksQuery.ToEntityArray(Allocator.TempJob),
            GreenTanks = greenTanksQuery.ToEntityArray(Allocator.TempJob),
            GreenTanksPositions = greenTanksQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
            Ecb = ecb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),


        }.ScheduleParallel();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


}

[BurstCompile]
public partial struct TankAimJob : IJobEntity
{
    [ReadOnly]
    public NativeArray<LocalToWorld> RedTanksPositions, GreenTanksPositions;
    [ReadOnly]
    public NativeArray<Entity> RedTanks, GreenTanks;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public Unity.Mathematics.Random Random;
    [BurstCompile]
    public void Execute(TankAspect tank, [ChunkIndexInQuery] int sortkey)
    {

        Entity selectedTank; //Inimigo selecionado pelo radar do tanque
        LocalToWorld selectedTarget;

        //Buscando tanque inimigo para selecionar (será modificado para pegar somente quem está na vista)
        if (tank.Team == Team.Red)
        {
            var randomIndex = Random.NextInt(0, GreenTanks.Length);
            selectedTarget = GreenTanksPositions[randomIndex];
            selectedTank = GreenTanks[randomIndex];
        }
        else
        {
            var randomIndex = Random.NextInt(0, RedTanks.Length);
            selectedTarget = RedTanksPositions[randomIndex];
            selectedTank = RedTanks[randomIndex];
        }

        //Debug.Log($"{tank.Entity} Mirando em {selectedTank}");
        tank.SetAimTo(selectedTarget.Position); //Move a malha
        Ecb.SetComponentEnabled<TankAttack>(sortkey, tank.Entity, true); //Ativa o componenete de ataque, que vai ser processado em AttackSystem
        tank.Attack.ValueRW.Target = selectedTank; //Registra o tanque inimigo escolhido para mirar
        
        //Não é mais um tanque livre
        Ecb.SetComponentEnabled<StandbyTankTag>(sortkey, tank.Entity, false);

    }
}

