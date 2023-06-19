using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TankSpawnerMono : MonoBehaviour
{
    [Tooltip("Espaço que cada tanque ocupa")]
    public Vector2 BlockSize;
    public Vector2 Start; 
    public Vector2 End;
    [Tooltip("Proporção que cada tipo de tanque deve spawnar")]
    public SpawnRate[] SpawnRate;
    [Tooltip("Rotação em graus do tanque ao spawnar")]
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
            Orientation = authoring.Orientation * math.PI / 180f
        });

        AddComponent<RandomGenerator>(entity, new RandomGenerator
        {
            Value = new Unity.Mathematics.Random((uint)authoring.RandomSeed)
        });

        AddBuffer<TankSpawnerRateBuffer>(entity);

        for (int i = 0; i < authoring.SpawnRate.Length; i++)
        {
            AppendToBuffer(entity, new TankSpawnerRateBuffer
            {
                Kind = authoring.SpawnRate[i].Kind,
                Prefab = GetEntity(authoring.SpawnRate[i].Prefab, TransformUsageFlags.Dynamic),
                Weight = authoring.SpawnRate[i].Weight,
            });
        }



    }
}

