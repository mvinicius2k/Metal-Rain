using System;
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


/// <summary>
/// Escaneia os tanques inimigos e obtem o mais próximo de acordo com uma precisão de cada tipo de tanque.
/// Basicamente a IA dos tanques
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct RadarSystem : ISystem
{
    private bool started;
    private bool endgame;
    private EntityQuery redTanks;
    private EntityQuery greenTanks;
    private EntityQuery allTanks;
    private TankAspect.TypeHandle tankAspectHandle;
    private ComponentLookup<LocalToWorld> globalTransformLookup;

    //Layer em que os tanques operam. Burst nao aceita string.
    private const uint TankLayer = (uint)Layer.Tank;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        tankAspectHandle = new TankAspect.TypeHandle(ref state);
        globalTransformLookup = state.GetComponentLookup<LocalToWorld>();

        //Obtendo todos os tanques. EntityQueryOptions.IgnoreComponentEnabledState é necessário para não ignorar as tags desativadas
        greenTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<GreenTeamTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(ref state);
        redTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithAll<RedTeamTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(ref state);
        allTanks = new EntityQueryBuilder(Allocator.TempJob).WithAspect<TankAspect>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(ref state);
        state.RequireForUpdate<TankProperties>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (endgame)
            return;

        tankAspectHandle.Update(ref state);
        globalTransformLookup.Update(ref state);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            started = true;
        }

        if (!started)
            return;


        //Se não houver nenhum tanque em algum dos lados, gameover
        if (redTanks.IsEmpty || greenTanks.IsEmpty)
        {
            endgame = true;
            var winner = redTanks.IsEmpty ? "verde" : "vermelho";
            Debug.Log($"Fim de jogo. Time {winner} venceu!");
            return;
        }

        //Obtendo singleton de física para tracar os raycasts para saber quais tanques são atingíveis
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        var greenTanksCount = greenTanks.CalculateEntityCount();
        var redTanksCount = redTanks.CalculateEntityCount();

        //Obtendo colisores
        var greenColliders = new NativeArray<Entity>(greenTanksCount, Allocator.TempJob);
        var redColliders = new NativeArray<Entity>(redTanksCount, Allocator.TempJob);
        int greenIdx = 0, redIdx = 0;
        foreach (var tank in SystemAPI.Query<TankAspect>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
        {

            if (tank.Team == Team.Green)
                greenColliders[greenIdx++] = tank.ModelEntity;
            else
                redColliders[redIdx++] = tank.ModelEntity;
        }

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var job = new TankRadarJob
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
#if MAINTHREAD
            SingleEcb = ecb,
#else
            Ecb = ecb.AsParallelWriter(),
#endif
            Random = new Unity.Mathematics.Random(50), //Usado para escolher um tanque dentre os que, de acordo com a precisão, foram obtidos
            TankAspectTypeHandle = tankAspectHandle,
            EnemyLayer = TankLayer,
            Physics = physicsWorld,
            DeltaTime = SystemAPI.Time.DeltaTime,
            GlobalTransformLookup = globalTransformLookup,
        };
#if MAINTHREAD
        job.Run(allTanks);
#elif SCHEDULE
        job.Schedule(allTanks, state.Dependency).Complete();
#else
        job.ScheduleParallel(allTanks, state.Dependency).Complete();
#endif

#if !MAINTHREAD
        state.Dependency.Complete();
#endif
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

    }

    [BurstCompile]
    public partial struct TankRadarJob : IJobChunk //IJobEntity dava incompatibilidades com o TypeHandle do mesmo aspect que iterava
    {
        [ReadOnly]
        public TargetProperties RedTargets, GreenTargets;
        public TankAspect.TypeHandle TankAspectTypeHandle;
        public Unity.Mathematics.Random Random;
#if MAINTHREAD
        public EntityCommandBuffer SingleEcb;
#else
        public EntityCommandBuffer.ParallelWriter Ecb;
#endif
        [ReadOnly]
        public PhysicsWorld Physics;
        public uint EnemyLayer;
        public float DeltaTime;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> GlobalTransformLookup;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Todos os tanques inimigos ordenados pela distância deles até o <paramref name="tank"/></returns>
        [BurstCompile]
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
                    //Obtendo distância
                    var distance = math.distance(startPosition, enemies[j].GetWorldPosition(GlobalTransformLookup));


                    enemyDistances[flatIndex++] = new AimTarget
                    {
                        Distance = distance,
                        Entity = enemies[j].Entity,
                        Position = enemies[j].GetWorldPosition(GlobalTransformLookup)
                    };
                }

            }

            //Ordenando
            enemyDistances.Sort(new DistanceComparer());

            return enemyDistances;
        }

        /// <returns>Verdadeiro se tiver 1 Tanque disponível para atirar</returns>
        [BurstCompile]
        public bool TryChoose(in TankAspect tank, in NativeArray<AimTarget> enemies, in NativeArray<Entity> enemyModels, int accuracy, out AimTarget chosed)
        {
            //Lista de tamanho de acordo com a precisão. Um tanque destes será escolhido.
            var mostClosest = new NativeList<AimTarget>(accuracy, Allocator.Temp);

            //Trançando raycast para ver se o tanque é atingível
            for (int i = 0; i < enemies.Length; i += accuracy)
            {
                var input = new RaycastInput
                {
                    Start = tank.GetWorldPosition(in GlobalTransformLookup),
                    End = enemies[i].Position,

                    //Ambos precisam estar na mesma camada física
                    Filter = new CollisionFilter
                    {
                        BelongsTo = EnemyLayer, 
                        CollidesWith = EnemyLayer,
                    }
                };

                Debug.DrawRay(input.Start, input.End - input.Start, tank.Team == Team.Green ? Color.green : Color.red, 1f);

                //Ignorando quando é detectado o próprio tanque (raycast saindo de dentro do colisor)
                var collector = new IgnoreEntitiesCollector(tank.ModelEntity);

                var sucess = Physics.CastRay(input, ref collector);
                if (sucess && enemyModels.Contains(collector.ClosestHit.Entity))
                {
                    //Tanque disponível, adding
                    mostClosest.Add(enemies[i]);

                    //Se alcançou a precisão ou nao tem mais tanque, retornando resultados
                    if (mostClosest.Length == accuracy || i == mostClosest.Length - 1)
                    {
                        chosed = mostClosest[Random.NextInt(0, mostClosest.Length)];
                        return true;
                    }

                }

            }


            //Nenhum disponível
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

                //Carrengando radar
                if (tank.Attack.ValueRO.RadarTimer > 0f)
                {
                    tank.Attack.ValueRW.RadarTimer -= DeltaTime;
                    continue;
                }

                //Radar está carregando

                
                if (!tank.IsFree)
                    continue; //Já tem um alvo, esperando...
                else
                    tank.Attack.ValueRW.RadarTimer = tank.RadarDelay; //Iniciando scan, resetando cronometro de carregamento

                var targets = tank.Team == Team.Green ? GreenTargets : RedTargets;

                //Obtém tanques inimigos ordenados pela distância (inclusive os inalcansáveis)
                var enemies = RadarTank(tank, targets.TankChunks, targets.Count);

                //Tentando achar 1 tanque inimigo
                var sucess = TryChoose(in tank, in enemies, targets.Models, tank.RadarAccuracy, out var enemy);

                //Não achou, talvez por que os tanque do time estão bloqueando sua visão. Esperar scan carregar para tentar de novo...
                if (!sucess)
                    continue;

                //Achou. Rotacionando tanque
                tank.SetAimTo(enemy.Position, GlobalTransformLookup);

                tank.Attack.ValueRW.Target = enemy.Entity;
#if MAINTHREAD
                SingleEcb.SetComponentEnabled<TankAttack>(tank.Entity, true);
                SingleEcb.SetComponentEnabled<StandbyTankTag>(tank.Entity, false);
#else
                Ecb.SetComponentEnabled<TankAttack>(unfilteredChunkIndex, tank.Entity, true);
                Ecb.SetComponentEnabled<StandbyTankTag>(unfilteredChunkIndex, tank.Entity, false);
#endif

            }
        }
    }
}


