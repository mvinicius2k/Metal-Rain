using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using RaycastHit = Unity.Physics.RaycastHit;

[
    BurstCompile,
    UpdateInGroup(typeof(FixedStepSimulationSystemGroup)),
    UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct RadarSystem : ISystem
{
    //private const int TankAccuracy = 3;

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
        if (redTanks.IsEmpty || greenTanks.IsEmpty)
        {
            endgame = true;
            Debug.Log("Fim de jogo");
            return;
        }

        freeGreenTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<GreenTeamTag>().WithAll<StandbyTankTag>().Build(ref state);
        freeRedTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<RedTeamTag>().WithAll<StandbyTankTag>().Build(ref state);



        var redSingleton = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        var redEcb = redSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        var greenTanksCount = greenTanks.CalculateEntityCount();
        var redTanksCount = redTanks.CalculateEntityCount();

        var greenColliders = new NativeArray<Entity>(greenTanksCount, Allocator.TempJob);
        var redColliders = new NativeArray<Entity>(redTanksCount, Allocator.TempJob);

        int greenIdx = 0, redIdx = 0;
        foreach (var tank in SystemAPI.Query<TankAspect>())
        {
            if (tank.Team == Team.Green)
                greenColliders[greenIdx++] = tank.ModelEntity;
            else
                redColliders[redIdx++] = tank.ModelEntity;
        }


        var redJob = new TankRadarJob
        {
            EnemyModels = greenColliders,
            EnemiesChunks = greenTanks.ToArchetypeChunkArray(Allocator.TempJob),
            Ecb = redEcb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            EnemiesCount = greenTanksCount,
            EnemyLayer = ((uint)LayerMask.GetMask(Constants.LayerTank)),
            Physics = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime
            

        }.ScheduleParallel(freeRedTanks, state.Dependency);

        redJob.Complete();

        var greenSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        var greenEcb = greenSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var greenJob = new TankRadarJob
        {
            EnemyModels = redColliders,
            EnemiesChunks = redTanks.ToArchetypeChunkArray(Allocator.TempJob),
            Ecb = greenEcb.AsParallelWriter(),
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            EnemiesCount = redTanksCount,
            EnemyLayer = ((uint)LayerMask.GetMask(Constants.LayerTank)),
            Physics = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime
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

//    [BurstCompile]
public partial struct TankRadarJob : IJobChunk
{
    [ReadOnly]
    public NativeArray<Entity> EnemyModels;
    [ReadOnly]
    public NativeArray<ArchetypeChunk> EnemiesChunks;
    public TankAspect.TypeHandle TankAspectTypeHandle;
    public Unity.Mathematics.Random Random;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public int EnemiesCount;
    [ReadOnly]
    public PhysicsWorld Physics;
    public uint EnemyLayer;
    public float DeltaTime;

    //private NativeArray<AimTarget> EnemyDistances;
    public NativeArray<AimTarget> RadarTank(TankAspect tank, int sortkey = 0)
    {
        //var targets = new NativeArray<AimTarget>(Accuracy, Allocator.Temp);
        var startPosition = tank.LocalTransform.ValueRO.Position;
        var enemyDistances = new NativeArray<AimTarget>(EnemiesCount, Allocator.Temp);
        var flatIndex = 0;

        for (int i = 0; i < EnemiesChunks.Length; i++)
        {
            var enemies = TankAspectTypeHandle.Resolve(EnemiesChunks[i]);
            for (int j = 0; j < enemies.Length; j++)
            {
                var distance = math.distance(startPosition, enemies[j].Position);
                enemyDistances[flatIndex++] = new AimTarget
                {
                    Distance = distance,
                    Entity = enemies[j].Entity,
                    Position = enemies[j].Position
                };

                //for (int k = 0; k < targets.Length; k++)
                //{
                //    if (distance < targets[k].Distance || targets[k].Entity == Entity.Null)
                //    {
                //        targets[k] = new AimTarget { 
                //            Distance = distance,
                //            Position = enemiesChunk[j].Position,
                //            Entity = enemiesChunk[j].Entity
                //        };

                //        targets.Sort(new DistanceComparer());
                //        break;
                //    }
                //}
            }

        }

        enemyDistances.Sort(new DistanceComparer());
        return enemyDistances;
        //var index = Random.NextInt(0, targets.Length);
        //var target = targets[index];


        //tank.SetAimTo(target.Position); //Move a malha
        //                                //Ecb.SetComponent(unfilteredChunkIndex, tank.ModelEntity, new ModelLookAt { Target = target.Position });
        //                                //Ecb.SetComponentEnabled<ModelLookAt>(unfilteredChunkIndex, tank.ModelEntity, true);

        //Ecb.SetComponentEnabled<TankAttack>(unfilteredChunkIndex, tank.Entity, true); //Ativa o componenete de ataque, que vai ser processado em AttackSystem
        //                                                                              //Registra o tanque inimigo escolhido para mirar
        //tank.Attack.ValueRW.Target = target.Entity;

        ////Não é mais um tanque livre
        //Ecb.SetComponentEnabled<StandbyTankTag>(unfilteredChunkIndex, tank.Entity, false);
    }

    public bool TryChoose(in TankAspect tank, in NativeArray<AimTarget> enemies, int accuracy, out AimTarget chosed)
    {

        var mostClosest = new NativeList<AimTarget>(accuracy, Allocator.Temp);
        for (int i = 0; i < enemies.Length; i += accuracy)
        {
            var input = new RaycastInput
            {
                Start = tank.Position,
                End = enemies[i].Position,
                Filter = new CollisionFilter
                {
                    BelongsTo = EnemyLayer,
                    CollidesWith = EnemyLayer,
                    
                }
            };

            var collector = new IgnoreEntitiesCollector(new NativeArray<Entity>(new Entity[] { tank.ModelEntity }, Allocator.Temp));

            var sucess = Physics.CastRay(input, ref collector);
            if (sucess && EnemyModels.Contains(collector.ClosestHit.Entity))
            {

                Debug.Log($"{tank.Entity} sucesso para {collector.ClosestHit.Entity}");
                mostClosest.Add(enemies[i]);

                if (mostClosest.Length == accuracy || i == mostClosest.Length - 1)
                {
                    chosed = mostClosest[Random.NextInt(0, mostClosest.Length)];
                    return true;
                }

            }
            else
            {
                Debug.Log($"{tank.Entity} bloqueado por {collector.ClosestHit.Entity}");
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

            if(tank.Attack.ValueRO.RadarTimer > 0f)
            {
                tank.Attack.ValueRW.RadarTimer -= DeltaTime;
                continue;
            }

            

            if (tank.AimLocked)
            {
                Debug.Log($"{tank.Entity} já está mirando");
                continue;
            } else
            {
                Debug.Log($"{tank.Entity} procurando");
                tank.Attack.ValueRW.RadarTimer = tank.RadarDelay;
            }

            var enemies = RadarTank(tank);

            var sucess = TryChoose(in tank, in enemies, tank.RadarAccuracy, out var enemy);

            if (!sucess)
                continue;


            tank.SetAimTo(enemy.Position); //Move a malha

            Ecb.SetComponentEnabled<TankAttack>(unfilteredChunkIndex, tank.Entity, true); //Ativa o componenete de ataque, que vai ser processado em AttackSystem
                                                                                          //Registra o tanque inimigo escolhido para mirar
            tank.Attack.ValueRW.Target = enemy.Entity;

            //Não é mais um tanque livre
            Ecb.SetComponentEnabled<StandbyTankTag>(unfilteredChunkIndex, tank.Entity, false);


            ////EnemyDistances = new NativeArray<AimTarget>(EnemiesCount, Allocator.Temp);

            ////var targets = new NativeArray<AimTarget>(Accuracy, Allocator.Temp);
            //var startPosition = tank.LocalTransform.ValueRO.Position;

            //var enemyDistances = new NativeArray<AimTarget>(EnemiesChunks.Length, Allocator.Temp);
            //for (int i = 0; i < EnemiesChunks.Length; i++)
            //{
            //    var enemyChunk = EnemiesChunks[i];
            //    var enemiesChunk = TankAspectTypeHandle.Resolve(enemyChunk);

            //    for (int j = 0; j < enemiesChunk.Length; j++)
            //    {
            //        var distance = math.distance(startPosition, enemiesChunk[j].Position);

            //        //for (int k = 0; k < targets.Length; k++)
            //        //{
            //        //    if (distance < targets[k].Distance || targets[k].Entity == Entity.Null)
            //        //    {
            //        //        targets[k] = new AimTarget { 
            //        //            Distance = distance,
            //        //            Position = enemiesChunk[j].Position,
            //        //            Entity = enemiesChunk[j].Entity
            //        //        };

            //        //        targets.Sort(new DistanceComparer());
            //        //        break;
            //        //    }
            //        //}
            //    }

            //}



            // var ceil = math.min(enemies.Length, Accuracy);
            //var index = Random.NextInt(0, enemies.Length);
            //var target = enemies[index];



        }
    }
}

public struct DistanceComparer : IComparer<AimTarget>
{
    public int Compare(AimTarget x, AimTarget y)
    => x.Distance.CompareTo(y.Distance);
}
public struct IgnoreEntitiesCollector : ICollector<RaycastHit>
{
    public NativeArray<Entity> Ignore;

    public bool EarlyOutOnFirstHit => false;
    public RaycastHit ClosestHit;
    public float MaxFraction { get; private set; }

    public int NumHits { get; private set; }

    public IgnoreEntitiesCollector(NativeArray<Entity> ignore)
    {
        ClosestHit = default;
        Ignore = ignore;
        MaxFraction = 1f;
        NumHits = 0;
    }

    public bool AddHit(RaycastHit hit)
    {

        if (!Ignore.Contains(hit.Entity))
        {
            ClosestHit = hit;
            return true;
        }
        else
            return false;
    }
}
