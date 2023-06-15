using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TankSpawnerMono : MonoBehaviour
{
    
    public Vector2 BlockSize, Start, End;
    public SpawnRate[] SpawnRate;
    public float Orientation;
    public Team Team;
    public int RandomSeed = 50;

    private void OnDrawGizmos()
    {
        Gizmos.color = Team == Team.Green ? CustomColors.alphaGreen : CustomColors.alphaRed;
        var center = new Vector3
        {
            x = transform.position.x + ((Start.x + End.x) / 2f),
            y = transform.position.y,
            z = transform.position.z + ((Start.y + End.y) / 2f)
        };

        var size = new Vector3
        {
            x = Mathf.Abs(Start.x - End.x),
            y = transform.position.y,
            z = Mathf.Abs(Start.y - End.y)
        };
        
        //
        Gizmos.DrawCube(center, size);
    }

}

public class TankSpawnerMonoBaker : Baker<TankSpawnerMono>
{
    public override void Bake(TankSpawnerMono authoring)
    {
        var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
        AddComponent(entity, new TankSpawner
        {
            BlockSize = authoring.BlockSize,
            Start = authoring.Start,
            End = authoring.End,
            Team = authoring.Team,
            Orientation = authoring.Orientation * math.PI/180f
        }) ;

        AddComponent<RandomGenerator>(entity, new RandomGenerator
        {
            Value = new Unity.Mathematics.Random((uint)authoring.RandomSeed)
        }) ;

        AddBuffer<TankSpawnerRateBuffer>(entity);

        for (int i = 0; i < authoring.SpawnRate.Length; i++)
        {
            AppendToBuffer(entity, new TankSpawnerRateBuffer
            {
                Kind = authoring.SpawnRate[i].Kind,
                Prefab = GetEntity(authoring.SpawnRate[i].TankPrefab, TransformUsageFlags.Dynamic),
                Weight = authoring.SpawnRate[i].Weight,
            }) ;
        }

        

    }
}

