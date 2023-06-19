using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

/// <summary>
/// Movimenta as balas e aplica dano no tanque que alguma bala colidir
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct BulletSystem : ISystem
{
    private ComponentLookup<TankDefense> defenseLookup;
    private ComponentLookup<LocalToWorld> globalTransformLookup;
    private ComponentLookup<Parent> parentLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        defenseLookup = state.GetComponentLookup<TankDefense>();
        globalTransformLookup = state.GetComponentLookup<LocalToWorld>();
        parentLookup = state.GetComponentLookup<Parent>();  
        state.RequireForUpdate<Bullet>();


    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.CompleteDependency();
        defenseLookup.Update(ref state);
        globalTransformLookup.Update(ref state);
        parentLookup.Update(ref state);

        //Obtendo estruturas para usar física
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var physicsEcb = new EntityCommandBuffer(Allocator.TempJob);

        var job = new BulletJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            Speed = 15f, //Velicidade padrão das balas
            DefenseLookup = defenseLookup,
            GlobalTransformLookup = globalTransformLookup,
            ParentLookup = parentLookup,
            Physics = physicsWorld,
#if MAINTHREAD
            SingleEcb = physicsEcb,
#else
            Ecb = physicsEcb.AsParallelWriter(),
#endif



        };
#if MAINTHREAD
        job.Run();
#elif SCHEDULE
        var handle = job.Schedule(state.Dependency);
#else
        var handle = job.ScheduleParallel(state.Dependency);
#endif

#if !MAINTHREAD
        state.Dependency = handle;

#endif
        state.Dependency.Complete();
        physicsEcb.Playback(state.EntityManager);
        physicsEcb.Dispose();


    }


    [BurstCompile]
    public partial struct BulletJob : IJobEntity
    {
        public float Speed, DeltaTime;
        [ReadOnly]
        public PhysicsWorld Physics;
        [ReadOnly]
        public ComponentLookup<Parent> ParentLookup;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> GlobalTransformLookup;
        [ReadOnly]
        public ComponentLookup<TankDefense> DefenseLookup;
#if MAINTHREAD
        public EntityCommandBuffer SingleEcb;
#else
        public EntityCommandBuffer.ParallelWriter Ecb;
#endif

        //Iterando sobre todas as balas
        [BurstCompile]
        public void Execute(BulletAspect bulletAspect, [ChunkIndexInQuery] int sortkey)
        {
            var globalTransform = GlobalTransformLookup.GetRefRO(bulletAspect.Entity);

            //List para armazenar os hits, a bala deve ser destruída no primeiro tanque que atingir
            var hits = new NativeList<DistanceHit>(Allocator.Temp);

            //Obtendo centro e rotação da bala para usar no overlap
            var center = GlobalTransformLookup.GetRefRO(bulletAspect.Bullet.ValueRO.Center).ValueRO.Position;
            var bulletRotation = globalTransform.ValueRO.Rotation;

            var sucess = Physics.OverlapBox(center, bulletRotation, bulletAspect.ColliderSize, ref hits, bulletAspect.Bullet.ValueRO.Layer);

            if (sucess)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    //Colidindo com o modelo e procurando o componente de defesa
                    if (ParentLookup.TryGetComponent(hits[i].Entity, out var parent)
                        && DefenseLookup.HasComponent(parent.Value))
                    {
                        //Aplicando dano
#if MAINTHREAD
                        SingleEcb.AppendToBuffer(parent.Value, new Damage
                        {
                            Value = bulletAspect.Bullet.ValueRO.Damage
                        });
                        SingleEcb.DestroyEntity(bulletAspect.Entity);
#else
                        Ecb.AppendToBuffer(sortkey, parent.Value, new Damage
                        {
                            Value = bulletAspect.Bullet.ValueRO.Damage
                        });
                        Ecb.DestroyEntity(sortkey, bulletAspect.Entity);
#endif
                    }
                }
            }
            else
            {
                //Nada colidiu... movimentando bala...
                var translationValue = Speed * DeltaTime * math.normalize(globalTransform.ValueRO.Forward);
                bulletAspect.LocalTransform.ValueRW.Position += translationValue;
                bulletAspect.Countdown.ValueRW.Value -= DeltaTime;

                //Tempo de vida acabou, destruindo-a
                if (bulletAspect.Countdown.ValueRO.Value <= 0f)
                {
#if MAINTHREAD
                    SingleEcb.DestroyEntity(bulletAspect.Entity);
#else
                    Ecb.DestroyEntity(sortkey, bulletAspect.Entity);
#endif
                }
            }

        }
    }
}
