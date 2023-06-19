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

/// <summary>
/// Usado para ler as configurações do .json do mundo mono para o mundo entities. Dispensável no jogo real.
/// </summary>
public partial struct StartupSystem : ISystem
{

    private SpawnSpawnFieldsAspect.Lookup aspectLookup;


    private Entity GetPrefab(TankKind kind, in DynamicBuffer<TankPrefabs> buffer)
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
        aspectLookup = new SpawnSpawnFieldsAspect.Lookup(ref state);
        state.RequireForUpdate<TankPrefabs>();
    }



    public void OnUpdate(ref SystemState state)
    {
        
        aspectLookup.Update(ref state);
       
        var singleonEntity = SystemAPI.GetSingletonEntity<SpawnField>();
        var aspect = aspectLookup[singleonEntity];


        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var spawnPrefab = aspect.StartSpawn.ValueRO.Prefab;
        foreach (var configs in StartParams.Instance.StartModel.SpawnConfig)
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

        state.EntityManager.DestroyEntity(singleonEntity);
    }
}

