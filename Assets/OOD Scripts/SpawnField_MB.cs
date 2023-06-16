using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct TankKind_Prefab
{
    public TankKind Kind;
    public GameObject Prefab;
}
[Serializable]
public struct SpawnRate
{
    public int Weight;
    public TankKind Kind;
    public GameObject Prefab;

    public int GetTotalFrom(float size, float totalWeight)
    {
        var rate = Weight / totalWeight;
        return (int)math.ceil(size * rate);
    }
}

public enum TankKind
{
    Balanced = 0,
    Hunter = 1,
    Light = 2
}



public class SpawnField_MB : MonoBehaviour
{

    public Vector2 BlockSize, StartAt, EndAt;
    public SpawnRate[] SpawnRates;
    public Team Team;
    [Range(-360f, 360f)]
    public float Orientation;
    private List<Tank_MB> tanks;
    private int xLimit, zLimit;
    private Team enemyTeam;
    public Team EnemyTeam => enemyTeam;
    private string enemyTag;
    public string EnemyTag => enemyTag;
    public int Size => zLimit * xLimit;
    private int weightSum;

    public int CalcWeightSum()
    {
        var count = 0;
        for (int i = 0; i < SpawnRates.Length; i++)
        {
            count += SpawnRates[i].Weight;
        }
        return count;
    }

    public string TeamName => Team.GetName(typeof(Team), Team);
    public List<Tank_MB> Tanks { get => tanks; }
    public int XLimit { get => xLimit; }
    public int ZLimit { get => zLimit; }
    public Unity.Mathematics.Random Random;



    public void FromDto(SpawnConfigDto config)
    {
        transform.position = config.WorldPosition;
        BlockSize = config.BlockSize;
        StartAt = config.Start;
        EndAt = config.End;
        Orientation = config.Orientation;
        SpawnRates = new SpawnRate[config.SpawnRates.Length];
        for (int i = 0; i < SpawnRates.Length; i++)
        {
            var kind = (TankKind)config.SpawnRates[i].Kind;
            var prefab = StartParams.Instance.GetTankPrefab(kind);
            SpawnRates[i] = new SpawnRate
            {
                Kind = kind,
                Weight = config.SpawnRates[i].Weight,
                Prefab = prefab
            };
        }
        Team = config.Team;
        Random = new Unity.Mathematics.Random((uint)config.RandomSeed);

        Init();
    }

    public void Spawn()
    {

        var list = new List<GameObject>(Size);
        var xCoeficient = EndAt.x >= StartAt.x ? 1f : -1f;
        var zCoeficient = EndAt.y >= StartAt.y ? 1f : -1f;

        var flatIdx = 0;
        for (int i = 0; i < SpawnRates.Length; i++)
        {
            var total = SpawnRates[i].GetTotalFrom(Size, CalcWeightSum());
            for (int j = 0; j < total; j++)
            {
                list.Add(SpawnRates[i].Prefab);
                if (list.Count == list.Capacity)
                    goto RandomizeList;
            }
        }

    RandomizeList:
        Random.Shuffle(list);

        flatIdx = 0;
        for (int i = 0; i < xLimit; i++)
        {
            for (int z = 0; z < zLimit; z++)
            {
                var prefab = list[flatIdx++];
                var newTank = Instantiate(prefab, transform);
                newTank.transform.position = new Vector3
                {
                    x = transform.position.x + i * BlockSize.x * xCoeficient,
                    y = transform.position.y,
                    z = transform.position.z + z * BlockSize.y * zCoeficient,
                };
                newTank.transform.eulerAngles = new Vector3(0f, Orientation, 0f);
                var tankMb = newTank.GetComponent<Tank_MB>();
                tankMb.Dependencies(this);
                tankMb.OnDead.AddListener((tank) => SpawnFieldManager.Instance.TanksProperties[Team].DeadCount++);
                tanks.Add(tankMb);
            }
        }


    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        xLimit = Mathf.RoundToInt(Mathf.Abs((StartAt.x - EndAt.x)) / BlockSize.x);
        zLimit = Mathf.RoundToInt(Mathf.Abs((StartAt.y - EndAt.y)) / BlockSize.y);
        var total = xLimit * zLimit;
        if (tanks == null)
            tanks = new List<Tank_MB>(xLimit * zLimit);
        else
            tanks.Capacity = total;

        enemyTeam = Team == Team.Green ? Team.Red : Team.Green;
        enemyTag = Team == Team.Green ? Constants.TagRedTeam : Constants.TagGreenTeam;
    }

    private void Start()
    {
        weightSum = CalcWeightSum();
    }

    private void OnDrawGizmos()
    {
        var bak = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.color = Team == Team.Green ? CustomColors.alphaGreen : CustomColors.alphaRed;
        var center = new Vector3
        {
            x = ((StartAt.x + EndAt.x) / 2f),
            y = 0f,
            z = ((StartAt.y + EndAt.y) / 2f)
        };
        var size = new Vector3
        {
            x = Mathf.Abs(StartAt.x - EndAt.x),
            y = 0.5f,
            z = Mathf.Abs(StartAt.y - EndAt.y)
        };
        //
        Gizmos.DrawCube(center, size);

        Gizmos.matrix = bak;
    }




}
