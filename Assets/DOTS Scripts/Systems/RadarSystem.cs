using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

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

        //var singleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        //var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);
        //Usar sync points

        //var transformTypeHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>(true);
        //var entityTypeHandle = SystemAPI.GetEntityTypeHandle();
        //var redSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        //var redEcb = redSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //var redJob = new TankRadarJob
        //{
        //    Enemies = greenTanks.ToArchetypeChunkArray(Allocator.TempJob),
        //    Ecb = redEcb,
        //    Random = new Unity.Mathematics.Random(50),
        //    //TransformTypeHandle = transformTypeHandle,
        //    //EntityTypeHandle = entityTypeHandle,
        //    TankAspectTypeHandle = new TankAspect.TypeHandle(ref state)

        //};

        //redJob.Run(freeRedTanks);


        //state.Dependency.Complete();
        //ecb.Playback(state.EntityManager);
        //ecb.Dispose();

        var redSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var redEcb = redSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        new TankRadarJob
        {
            Enemies = greenTanks.ToArchetypeChunkArray(Allocator.TempJob),
            Ecb = redEcb,
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            Accuracy = math.min(TankAccuracy, greenTanks.CalculateEntityCount())

        }.Run(freeRedTanks);

        var greenSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var greenEcb = greenSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        new TankRadarJob
        {
            Enemies = redTanks.ToArchetypeChunkArray(Allocator.TempJob),
            Ecb = greenEcb,
            Random = new Unity.Mathematics.Random(50),
            TankAspectTypeHandle = new TankAspect.TypeHandle(ref state),
            Accuracy = math.min(TankAccuracy, redTanks.CalculateEntityCount())

        }.Run(freeGreenTanks);

        //greenJob.Run(freeGreenTanks);

        //state.Dependency.Complete();
        //ecb.Playback(state.EntityManager);
        //ecb.Dispose();
    }




}

public struct AimTarget
{
    public Entity Entity;
    public float3 Position;
    public float Distance;

    
}

public partial struct TankRadarJob : IJobChunk
{
    [ReadOnly]
    public NativeArray<ArchetypeChunk> Enemies;
    //[ReadOnly]
    //public ComponentTypeHandle<LocalTransform> TransformTypeHandle;
    //[ReadOnly]
    public TankAspect.TypeHandle TankAspectTypeHandle;
    //[ReadOnly]
    //public EntityTypeHandle EntityTypeHandle;
    public Unity.Mathematics.Random Random;
    public EntityCommandBuffer Ecb;
    public int Accuracy;

    //[BurstCompile]
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        Debug.Log($"Accuray: {Accuracy}");
        var tanks = TankAspectTypeHandle.Resolve(chunk);
        
        for (int tankIndex = 0; tankIndex < tanks.Length; tankIndex++)
        {
            var tank = tanks[tankIndex];
            var targets = new NativeArray<AimTarget>(Accuracy, Allocator.Temp);
            var startPosition = tank.LocalTransform.ValueRO.Position;
            //RadarSystem.ClosestTanks(tank.LocalTransform.ValueRO.Position, in Enemies, ref targets, TransformTypeHandle, EntityTypeHandle);
            //var distances = new NativeArray<float>(targets.Length, Allocator.Temp);

            for (int i = 0; i < Enemies.Length; i++)
            {
                var enemyChunk = Enemies[i];

                var enemiesChunk = TankAspectTypeHandle.Resolve(enemyChunk);
                //var entitiesEnemy = currentChunk.GetNativeArray(EntityTypeHandle);
                //var localTransforms = currentChunk.GetNativeArray(ref TransformTypeHandle);

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
            Ecb.SetComponentEnabled<TankAttack>(tank.Entity, true); //Ativa o componenete de ataque, que vai ser processado em AttackSystem
            //Registra o tanque inimigo escolhido para mirar
            tank.Attack.ValueRW.Target = target.Entity;

            //Não é mais um tanque livre
            Ecb.SetComponentEnabled<StandbyTankTag>(tank.Entity, false);
        }
    }
}

public struct DistanceComparer : IComparer<AimTarget>
{
    public int Compare(AimTarget x, AimTarget y)
    => x.Distance.CompareTo(y.Distance);
}

//[BurstCompile]
//public partial struct TankAimJob : IJobFor
//{
//    //[ReadOnly]
//    //public NativeHashMap<int, UnsafeList<(Entity, LocalToWorld)>> Tanks;
//    [ReadOnly]
//    public NativeArray<ArchetypeChunk> EnemyTanksChunks;
//    [ReadOnly]
//    public ComponentTypeHandle<LocalTransform> TransformTypeHandle;
//    [ReadOnly]
//    public EntityTypeHandle EntityTypeHandle;
//    public EntityCommandBuffer Ecb;

//    public Unity.Mathematics.Random Random;
    
//    //[BurstCompile]
//    //public void Execute(TankAspect tank, [ChunkIndexInQuery] int sortkey)
//    //{
        

//    //    //var randomIndex = Random.NextInt(0, Tanks[team].Length);

//    //    //Entity selectedTank = Tanks[team][randomIndex].Item1; //Inimigo selecionado pelo radar do tanque
//    //    //LocalToWorld selectedTarget = Tanks[team][randomIndex].Item2; //Lugar dele
//    //    ////Buscando tanque inimigo para selecionar (será modificado para pegar somente quem está na vista)
//    //    ////var targets = new NativeArray<(int, float)>(3, Allocator.Temp);



//    //    ////for (int i = 0; i < enemyPositions.Length; i++)
//    //    ////{
//    //    ////    var distance = math.distance(startPosition, enemyPositions[i].Position);

//    //    ////    for (int j = 0; j < output.Length; j++)
//    //    ////    {
//    //    ////        if (distance < output[j].Item2)
//    //    ////        {
//    //    ////            output[j] = (i, distance);

//    //    ////        }
//    //    ////    }
//    //    ////}


//    //    ////Debug.Log($"{tank.Entity} Mirando em {selectedTank}");
//    //    //tank.SetAimTo(selectedTarget.Position); //Move a malha
//    //    //Ecb.SetComponentEnabled<TankAttack>(sortkey, tank.Entity, true); //Ativa o componenete de ataque, que vai ser processado em AttackSystem
//    //    //tank.Attack.ValueRW.Target = selectedTank; //Registra o tanque inimigo escolhido para mirar
        
//    //    ////Não é mais um tanque livre
//    //    //Ecb.SetComponentEnabled<StandbyTankTag>(sortkey, tank.Entity, false);
        
//    //}

//    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
//    {
//        var distances = new NativeArray<int>(EnemyTanksChunks.Length, Allocator.Temp);
//        var localTransforms = chunk.GetNativeArray(ref TransformTypeHandle);
//        var attackerEntities = chunk.GetNativeArray(EntityTypeHandle);

        

//        for (int ic = 0; ic < localTransforms.Length; ic++)
//        {
//            var startPosition = localTransforms[ic].Position;

//            Debug.Log($"Calculando distância de {startPosition}");

//            ClosestTanks(startPosition, EnemyTanksChunks.)    

//        }

//    }

//    public void Execute(int index)
//    {

//    }

//    private void ClosestTanks(float3 startPosition, in NativeArray<ArchetypeChunk> enemyChunks, ref NativeArray<int> output)
//    {
        
//        var distances = new NativeArray<float>(output.Length, Allocator.Temp);

//        for (int i = 0; i < enemyChunks.Length; i++)
//        {
//            var currentChunk = enemyChunks[i];



//        }

//        //for (int i = 0; i < enemyPositions.Length; i++)
//        //{
//        //    var distance = math.distance(startPosition, enemyPositions[i].Position);

//        //    for (int j = 0; j < output.Length; j++)
//        //    {
//        //        if (distance < output[j].Item2)
//        //        {
//        //            output[j] = (i, distance);

//        //        }
//        //    }
//        //}

//    }

