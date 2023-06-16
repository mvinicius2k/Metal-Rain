﻿using System;
using System.Linq;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public struct TankChildren
{
    public Entity Model;
    public LocalTransform FirePoint;


}

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct RadarSystem : ISystem
{
    private bool started;
    private bool endgame;
    private EntityQuery redTanks;
    private EntityQuery greenTanks;

    [BurstCompile]
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
        {
            started = true;
            //Debug.Log("Inicinaod");
        }

        if (!started)
            return;


        //Todos os tanks do time vermelho vivos

        greenTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<GreenTeamTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(ref state);
        redTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<RedTeamTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(ref state);

        //Se não houver nenhum tanque em algum dos lados, gameover
        if (redTanks.IsEmpty || greenTanks.IsEmpty)
        {
            endgame = true;
            Debug.Log("Fim de jogo");
            return;
        }

        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        var greenTanksCount = greenTanks.CalculateEntityCount();
        var redTanksCount = redTanks.CalculateEntityCount();

        var greenColliders = new NativeArray<Entity>(greenTanksCount, Allocator.TempJob);
        var redColliders = new NativeArray<Entity>(redTanksCount, Allocator.TempJob);

        var allTanks = SystemAPI.QueryBuilder().WithAspect<TankAspect>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build();

        int greenIdx = 0, redIdx = 0;
        foreach (var tank in SystemAPI.Query<TankAspect>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
        {

            if (tank.Team == Team.Green)
                greenColliders[greenIdx++] = tank.ModelEntity;
            else
                redColliders[redIdx++] = tank.ModelEntity;
        }

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        new TankRadarJob
        {
            GreenTargets = new TargetProperties
            {
                Count = redTanksCount,
                Models = redColliders,
                TankChunks = redTanks.ToArchetypeChunkArray(Allocator.TempJob),

            },
            RedTargets = new TargetProperties
            {
                Count = greenTanksCount,
                Models = greenColliders,
                TankChunks = greenTanks.ToArchetypeChunkArray(Allocator.TempJob),

            },
            Ecb = ecb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            EnemyLayer = (uint)Layer.Tank,
            Physics = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            GlobalTransformLookup = state.GetComponentLookup<LocalToWorld>(),
        }.ScheduleParallel(allTanks, state.Dependency).Complete();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();


    }
    public struct TargetProperties
    {
        public NativeArray<Entity> Models;
        public NativeArray<ArchetypeChunk> TankChunks;
        public int Count;

    }

    [BurstCompile]
    public partial struct TankRadarJob : IJobChunk
    {
        [ReadOnly]
        public TargetProperties RedTargets, GreenTargets;
        public TankAspect.TypeHandle TankAspectTypeHandle;
        public Unity.Mathematics.Random Random;
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly]
        public PhysicsWorld Physics;
        public uint EnemyLayer;
        public float DeltaTime;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> GlobalTransformLookup;

        public NativeArray<AimTarget> RadarTank(TankAspect tank, in NativeArray<ArchetypeChunk> enemiesChunks, int EnemiesCount)
        {
            var startPosition = tank.LocalTransform.ValueRO.Position;
            var enemyDistances = new NativeArray<AimTarget>(EnemiesCount, Allocator.Temp);
            var flatIndex = 0;

            for (int i = 0; i < enemiesChunks.Length; i++)
            {
                var enemies = TankAspectTypeHandle.Resolve(enemiesChunks[i]);
                for (int j = 0; j < enemies.Length; j++)
                {
                    var distance = math.distance(startPosition, enemies[j].GetWorldPosition(GlobalTransformLookup));
                    enemyDistances[flatIndex++] = new AimTarget
                    {
                        Distance = distance,
                        Entity = enemies[j].Entity,
                        Position = enemies[j].GetWorldPosition(GlobalTransformLookup)
                    };
                }

            }

            enemyDistances.Sort(new DistanceComparer());

            return enemyDistances;
        }

        public bool TryChoose(in TankAspect tank, in NativeArray<AimTarget> enemies, in NativeArray<Entity> enemyModels, int accuracy, out AimTarget chosed)
        {

            var mostClosest = new NativeList<AimTarget>(accuracy, Allocator.Temp);
            for (int i = 0; i < enemies.Length; i += accuracy)
            {
                var input = new RaycastInput
                {
                    Start = tank.GetWorldPosition(in GlobalTransformLookup),
                    End = enemies[i].Position,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = EnemyLayer,
                        CollidesWith = EnemyLayer,

                    }
                };

                Debug.DrawRay(input.Start, input.End - input.Start, tank.Team == Team.Green ? Color.green : Color.red, 1f);


                var collector = new IgnoreEntitiesCollector(tank.ModelEntity);

                var sucess = Physics.CastRay(input, ref collector);
                if (sucess && enemyModels.Contains(collector.ClosestHit.Entity))
                {

                    //Debug.Log($"{tank.Entity} sucesso para {collector.ClosestHit.Entity}");
                    mostClosest.Add(enemies[i]);

                    if (mostClosest.Length == accuracy || i == mostClosest.Length - 1)
                    {
                        chosed = mostClosest[Random.NextInt(0, mostClosest.Length)];
                        return true;
                    }

                }
                else
                {
                    //Debug.Log($"{tank.Entity} bloqueado por {collector.ClosestHit.Entity}");
                }

            }



            chosed = new AimTarget();
            return false;
        }

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var tanks = TankAspectTypeHandle.Resolve(chunk);



            for (int tankIndex = 0; tankIndex < tanks.Length; tankIndex++)
            {
                var tank = tanks[tankIndex];

                if (tank.Attack.ValueRO.RadarTimer > 0f)
                {
                    tank.Attack.ValueRW.RadarTimer -= DeltaTime;
                    if (tank.Team == Team.Red)
                    {
                        //Debug.Log($"Diminuído para {tank.Attack.ValueRO.RadarTimer}");
                    }
                    continue;
                }



                if (!tank.IsFree)
                {
                    //Debug.Log($"{tank.Entity} já está mirando");
                    continue;
                }
                else
                {
                    //Debug.Log($"{tank.Entity} procurando");
                    tank.Attack.ValueRW.RadarTimer = tank.RadarDelay;
                }

                var targets = tank.Team == Team.Green ? GreenTargets : RedTargets;

                var enemies = RadarTank(tank, targets.TankChunks, targets.Count);

                var sucess = TryChoose(in tank, in enemies, targets.Models, tank.RadarAccuracy, out var enemy);

                if (!sucess)
                    continue;


                tank.SetAimTo(enemy.Position, GlobalTransformLookup); //Move a malha

                tank.Attack.ValueRW.Target = enemy.Entity;
                Ecb.SetComponentEnabled<TankAttack>(unfilteredChunkIndex, tank.Entity, true);

                //Não é mais um tanque livre
                Ecb.SetComponentEnabled<StandbyTankTag>(unfilteredChunkIndex, tank.Entity, false);
                //Debug.Log($"{tank.Entity} mirará em {enemy.Entity}");
            }
        }
    }
}


