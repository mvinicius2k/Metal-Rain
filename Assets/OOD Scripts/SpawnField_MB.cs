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



public class SpawnField_MB : MonoBehaviour
{

    public Vector2 BlockSize, StartAt, EndAt;
    public GameObject ChosenTank;
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


    public  string TeamName => Team.GetName(typeof(Team), Team);
    public List<Tank_MB> Tanks { get => tanks; }
    public int XLimit { get => xLimit;}
    public int ZLimit { get => zLimit;}
    

    public void Spawn()
    {
        //var teamName = TeamName;
        //stopwatch.Start();
        for (int i = 0; i < xLimit; i++)
        {
            for (int z = 0; z < zLimit; z++)
            {
                var newTank = Instantiate(ChosenTank, transform);
                newTank.transform.position = new Vector3
                {
                    x = transform.position.x + i * BlockSize.x,
                    y = transform.position.y,
                    z = transform.position.z + z * BlockSize.y,
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
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Team == Team.Green ? CustomColors.alphaGreen : CustomColors.alphaRed;
        var center = new Vector3
        {
            x = transform.position.x + ((StartAt.x + EndAt.x) / 2f),
            y = transform.position.y,
            z = transform.position.z + ((StartAt.y + EndAt.y) / 2f)
        };
        var size = new Vector3
        {
            x = Mathf.Abs(StartAt.x - EndAt.x),
            y = 0.5f,
            z = Mathf.Abs(StartAt.y - EndAt.y)
        };

        //
        Gizmos.DrawCube(center, size);
    }




}
