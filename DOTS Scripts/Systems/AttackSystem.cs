using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Prepara a direção e rotação da bala para o inimigo que o radar pegou
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(VariableRateSimulationSystemGroup))]
public partial struct AttackSystem : ISystem
{
    private ComponentLookup<LocalToWorld> globalTransformLookup;
    private ComponentLookup<TankProperties> tankPropertiesLookup;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        tankPropertiesLookup = state.GetComponentLookup<TankProperties>();
        globalTransformLookup = state.GetComponentLookup<LocalToWorld>();
        state.RequireForUpdate<TankAttack>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        tankPropertiesLookup.Update(ref state);
        globalTransformLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var arr = new NativeList<(TankProperties, Entity)>(Allocator.Temp);
        foreach (var (properties, entity) in SystemAPI.Query<TankProperties>().WithNone<Damage>().WithEntityAccess())
        {
            arr.Add((properties, entity));
        }

        var job = new ApplyDamageJob
        {
#if MAINTHREAD
            SingleEcb = ecb,
#else
            Ecb = ecb.AsParallelWriter(),
#endif
            DeltaTime = SystemAPI.Time.DeltaTime,
            TransformLookup = globalTransformLookup,
            TankPropertiesLookup = tankPropertiesLookup


        };

#if !MAINTHREAD
        var handle = job.Schedule(state.Dependency);
        state.Dependency = handle;
#else
        job.Run();
#endif
        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public partial struct ApplyDamageJob : IJobEntity
    {
        public float DeltaTime;
#if MAINTHREAD
        public EntityCommandBuffer SingleEcb;
#else
        public EntityCommandBuffer.ParallelWriter Ecb;
#endif
        [ReadOnly]
        public ComponentLookup<LocalToWorld> TransformLookup;
        [ReadOnly]
        public ComponentLookup<TankProperties> TankPropertiesLookup;

        //Itera sobre todos os tanques que tem um ataque ativo e um alvo a ser atacado
        [BurstCompile]
        public void Execute(ApplyDamageAspect aspect, [ChunkIndexInQuery] int sortkey)
        {
            if (aspect.Timer > 0)
            {
                aspect.Timer -= DeltaTime;
                return;
            }

            //Instanciando bala
#if MAINTHREAD
            var bullet = SingleEcb.Instantiate(aspect.Properties.BulletPrefab);
#else
            var bullet = Ecb.Instantiate(sortkey, aspect.Properties.BulletPrefab);
#endif
            //De onde a bala vai sair
            var firepointTransform = TransformLookup.GetRefRO(aspect.Properties.FirePoint);

            //Posição do inimigo (centro)
            var enemyProperties = TankPropertiesLookup.GetRefRO(aspect.TargetEntity);
            var enemyTransform = TransformLookup.GetRefRO(enemyProperties.ValueRO.Center);

            //Apontando a bala para o inimigo
            var rot = TransformHelpers.LookAtRotation(firepointTransform.ValueRO.Position, enemyTransform.ValueRO.Position, math.up());
            var comp = new LocalTransform
            {
                Position = firepointTransform.ValueRO.Position,
                Rotation = rot,
                Scale = 1f
            };
#if MAINTHREAD
            SingleEcb.SetComponent(bullet, comp);
#else
            Ecb.SetComponent(sortkey, bullet, comp);
#endif
            //Reiniciando cronometro (aplicando cadencia)
            aspect.Timer = aspect.BaseProperties.Delay;

            ///A bala vai sair (transaladar) por outro sistema)
        }
    }
}