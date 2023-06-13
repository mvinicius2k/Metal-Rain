using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct SpawnRate
{
    public GameObject TankPrefab;
    public int Weight;
    public TankKind Kind;

    public int GetTotalFrom(float size, float totalWeight)
    {
        var rate = Weight / totalWeight;
        return (int) math.floor(size * rate);
    }
}

public enum TankKind
{
    Balanced,
    Hunter,
    Light
}



public class SpawnField_MB : MonoBehaviour
{

    public Vector2 BlockSize, StartAt, EndAt;
    public SpawnRate[] SpawnRates;
    public Team Team;
    [Range(-360f, 360f)]
    public float Orientation;

    private List<Tank_MB> tanks;
    //private Stopwatch stopwatch;
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

    public void Spawn()
    {

        var list = new GameObject[Size];
        var xCoeficient = EndAt.x >= StartAt.x ? 1f : -1f;
        var zCoeficient = EndAt.y >= StartAt.y ? 1f : -1f;

        var flatIdx = 0;
        for (int i = 0; i < SpawnRates.Length; i++)
        {
            var total = SpawnRates[i].GetTotalFrom(Size, CalcWeightSum());
            for (int j = 0; j < total; j++)
            {
                list[flatIdx++] = SpawnRates[i].TankPrefab;
            }
        }
        
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

        //stopwatch.Stop();

        //if (AnaliticsSetup.Instance == null) //Analitics desativado
        //    return;

        //var eventParams = new TankSpawnFieldModel
        //{
        //    elapsedTimeMs = (int)stopwatch.ElapsedMilliseconds,
        //    tanksCount = tanks.Count,
        //    team = teamName,
        //    DOTS = false
        //};

        //AnalyticsService.Instance.CustomData(TankSpawnFieldModel.EventName, eventParams.GetEventParams());
        //AnalyticsService.Instance.Flush();
    }

    private void Awake()
    {

        //    stopwatch = new Stopwatch();
        xLimit = Mathf.RoundToInt(Mathf.Abs((StartAt.x - EndAt.x)) / BlockSize.x);
        zLimit = Mathf.RoundToInt(Mathf.Abs((StartAt.y - EndAt.y)) / BlockSize.y);
        tanks = new List<Tank_MB>(xLimit * zLimit);
        enemyTeam = Team == Team.Green ? Team.Red : Team.Green;
        enemyTag = Team == Team.Green ? Constants.TagRedTeam : Constants.TagGreenTeam;
    }



    // Update is called once per frame
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
