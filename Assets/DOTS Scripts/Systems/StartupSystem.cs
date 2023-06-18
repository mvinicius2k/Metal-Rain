using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public partial struct StartupSystem : ISystem
{

    private EntityQuery prefabsQuery;
    bool stated;

    private Entity GetPrefab(TankKind kind, in DynamicBuffer<StartupPrefabs> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Kind == kind)
                return buffer[i].Prefab;

        }
        return Entity.Null;
    }

    public void OnCreate(ref SystemState state)
    {
        prefabsQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<StartupPrefabs>().Build(state.EntityManager);
        state.RequireForUpdate<StartupPrefabs>();
    }



    public void OnUpdate(ref SystemState state)
    {
        if (stated)
            return;


        SpawnSpawnFieldsAspect aspect;
        foreach (var item in SystemAPI.Query<SpawnSpawnFieldsAspect>())
        {
            aspect = item;
        }
        
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        Debug.Log("Criando spwnds");
        var spawnPrefab = aspect.StartSpawn.ValueRO.Prefab;
        foreach (var configs in StartParams.Instance.SpawnConfigs)
        {

            var entity = ecb.Instantiate(spawnPrefab);
            ecb.SetComponent(entity, new LocalTransform
            {
                Position = configs.WorldPosition,
                Rotation = quaternion.identity,
                Scale = 1f
            });

            ecb.SetComponent(entity, new TankSpawner
            {
                BlockSize = configs.BlockSize,
                Start = configs.Start,
                End = configs.End,
                Team = configs.Team,
                Orientation = configs.Orientation * math.PI / 180f
            });

            ecb.SetComponent<RandomGenerator>(entity, new RandomGenerator
            {
                Value = new Unity.Mathematics.Random((uint)configs.RandomSeed)
            });

            ecb.SetBuffer<TankSpawnerRateBuffer>(entity);

            for (int i = 0; i < configs.SpawnRates.Length; i++)
            {
                var kind = (TankKind)configs.SpawnRates[i].Kind;
                ecb.AppendToBuffer(entity, new TankSpawnerRateBuffer
                {
                    Kind = kind,
                    Prefab = GetPrefab(kind, in aspect.StartupPrefabs),
                    Weight = configs.SpawnRates[i].Weight,
                });
            }


        }

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        stated = true;
        
    }
}

