using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile, UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct RadarSystem : ISystem
{
    private const int TankAccuracy = 3;

    private bool started;
    private bool endgame;
    private EntityQuery redTanks, freeRedTanks;
    private EntityQuery greenTanks, freeGreenTanks;
    
    
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

        greenTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<GreenTeamTag>().Build(ref state);
        redTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<RedTeamTag>().Build(ref state);
        
        //Se não houver nenhum tanque em algum dos lados, gameover
        if(redTanks.IsEmpty || greenTanks.IsEmpty)
        {
            endgame = true;
            Debug.Log("Fim de jogo");
            return;
        }

        freeGreenTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<GreenTeamTag>().WithAll<StandbyTankTag>().Build(ref state);
        freeRedTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<RedTeamTag>().WithAll<StandbyTankTag>().Build(ref state);

        var redSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var redEcb = redSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var redJob = new TankRadarJob
        {
            Enemies = greenTanks.ToArchetypeChunkArray(Allocator.TempJob),
            Ecb = redEcb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            Accuracy = math.min(TankAccuracy, greenTanks.CalculateEntityCount())

        }.ScheduleParallel(freeRedTanks, state.Dependency);

        redJob.Complete();

        var greenSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var greenEcb = greenSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var greenJob = new TankRadarJob
        {
            Enemies = redTanks.ToArchetypeChunkArray(Allocator.TempJob),
            Ecb = greenEcb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            Accuracy = math.min(TankAccuracy, redTanks.CalculateEntityCount())

        }.ScheduleParallel(freeGreenTanks, redJob);

        greenJob.Complete();
    }




}

public struct AimTarget
{
    public Entity Entity;
    public float3 Position;
    public float Distance;

    
}

[BurstCompile]
public partial struct TankRadarJob : IJobChunk
{
    [ReadOnly]
    public NativeArray<ArchetypeChunk> Enemies;
    public TankAspect.TypeHandle TankAspectTypeHandle;
    public Unity.Mathematics.Random Random;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public int Accuracy;

    [BurstCompile]
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        var tanks = TankAspectTypeHandle.Resolve(chunk);
        
        for (int tankIndex = 0; tankIndex < tanks.Length; tankIndex++)
        {
            var tank = tanks[tankIndex];
            var targets = new NativeArray<AimTarget>(Accuracy, Allocator.Temp);
            var startPosition = tank.LocalTransform.ValueRO.Position;

            for (int i = 0; i < Enemies.Length; i++)
            {
                var enemyChunk = Enemies[i];

                var enemiesChunk = TankAspectTypeHandle.Resolve(enemyChunk);

                for (int j = 0; j < enemiesChunk.Length; j++)
                {
                    var distance = math.distance(startPosition, enemiesChunk[j].Position);
                    for (int k = 0; k < targets.Length; k++)
                    {
                        if (distance < targets[k].Distance || targets[k].Entity == Entity.Null)
                        {
                            targets[k] = new AimTarget { 
                                Distance = distance,
                                Position = enemiesChunk[j].Position,
                                Entity = enemiesChunk[j].Entity
                            };
                            
                            targets.Sort(new DistanceComparer());
                            break;
                        }
                    }
                }

            }
            var index = Random.NextInt(0, targets.Length);
            if(index == -1)
            {
                Debug.Log($"{targets.Length}");
            }
            var target = targets[index];


            tank.SetAimTo(target.Position); //Move a malha
            Ecb.SetComponentEnabled<TankAttack>(unfilteredChunkIndex, tank.Entity, true); //Ativa o componenete de ataque, que vai ser processado em AttackSystem
            //Registra o tanque inimigo escolhido para mirar
            tank.Attack.ValueRW.Target = target.Entity;

            //Não é mais um tanque livre
            Ecb.SetComponentEnabled<StandbyTankTag>(unfilteredChunkIndex, tank.Entity, false);
        }
    }
}

public struct DistanceComparer : IComparer<AimTarget>
{
    public int Compare(AimTarget x, AimTarget y)
    => x.Distance.CompareTo(y.Distance);
}

