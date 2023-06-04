using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Services.Analytics;
using UnityEditor.VersionControl;
using UnityEngine;


public class TanksProperty
{
    public List<SpawnField_MB> Spawns;
    public int DeadCount;
    public int SpawnedCount;

    public bool Empty => DeadCount >= SpawnedCount;
    
    public TanksProperty()
    {
        Spawns = new List<SpawnField_MB>();
    }
}

public class SpawnFieldManager : MonoBehaviour
{

    
    public Dictionary<Team,TanksProperty> TanksProperties;
    
    public Unity.Mathematics.Random Random;
    public GameObject SpawnFields;
    public bool EndGame;
    public bool Started;

    private static SpawnFieldManager instance;
    public static SpawnFieldManager Instance => instance;




    //public List<Tank_MB> GetAllTanks(Team team)
    //{
    //    if (!allTanks[team].obsolete)
    //        return allTanks[team].tanks;

    //    var fields = team == Team.Green ? GreenSpawns : RedSpawns;
    //    var listsize = fields.Count * fields.Sum(s => s.Tanks.Count);
    //    var list = new List<Tank_MB>(listsize);

    //    foreach (var spawn in fields)
    //    {
    //        list.AddRange(spawn.Tanks);
    //    }

    //    allTanks[team] = (true, list);
    //    return list;

    //}

    //public NativeArray<SpawnData> PrepareData(SpawnField_MB[] fields, Allocator allocator = Allocator.TempJob)
    //{
    //    var arrSize = fields.Sum(f => f.Size);
    //    var allData = new NativeArray<SpawnData>(arrSize, allocator);

    //    for (int i = 0; i < fields.Length; i++)
    //    {
    //        for (int x = 0; x < fields[i].XLimit; x++)
    //        {
    //            for (int z = 0; z < fields[i].ZLimit; z++)
    //            {
    //                var fieldPosition = fields[i].transform.position;
    //                var tankPosition = new float3
    //                {
    //                    x = fieldPosition.x + x * fields[i].BlockSize.x,
    //                    y = fieldPosition.y,
    //                    z = fieldPosition.z + z * fields[i].BlockSize.y
    //                };
    //                var data = new SpawnData
    //                {
    //                    Position = tankPosition,
    //                    Rotation = fields[i].Team == Team.Green ? new float3(0 , 180, 0) : float3.zero,
    //                    TankPrefab = fields[i].ChosenTank

    //                };
    //                var arrayIndex = i * fields.Length +  x * fields[i].XLimit + fields[i].ZLimit;
    //                allData[arrayIndex] = data;

    //            }
    //        }

    //    }

    //    return allData;
    //}

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Debug.Log("Iniciando spawns");
        var fields = SpawnFields.GetComponentsInChildren<SpawnField_MB>();
        TanksProperties = new(2);
        TanksProperties[Team.Green] = new();
        TanksProperties[Team.Red] = new();

        foreach (var field in fields)
            TanksProperties[field.Team].Spawns.Add(field);


    }

    private void OnSpawnChanged(SpawnField_MB spawn)
    {
        //allTanks[spawn.Team].obsolete = true;
    }

    private void Start()
    {
        //foreach (var spawn in GreenSpawns)
        //{
        //    spawn.OnTanksChangeCount.AddListener(On)
        //}
    }
    private void Update()
    {
        if (EndGame && Started)
            return;

        if (TanksProperties[Team.Red].Empty && Started)
        {
            EndGame = true;
            Debug.Log("Fim de jogo! Time verde venceu!");
        }
        else if (TanksProperties[Team.Green].Empty && Started)
        {
            EndGame = true;
            Debug.Log("Fim de jogo! Time vermelho venceu!");
        }
        
        if (Input.GetButtonDown(Constants.InputSpawnGreen))
        {
            foreach (var greenSpawn in TanksProperties[Team.Green].Spawns)
            {
                greenSpawn.Spawn();
                TanksProperties[Team.Green].SpawnedCount += greenSpawn.Tanks.Count;

            }

        }

        if (Input.GetButtonDown(Constants.InputSpawnRed))
        {
            foreach (var redSpawn in TanksProperties[Team.Red].Spawns)
            {
                redSpawn.Spawn();
                TanksProperties[Team.Red].SpawnedCount += redSpawn.Tanks.Count;
            }

        }

        Started = TanksProperties[Team.Red].SpawnedCount > 0 && TanksProperties[Team.Green].SpawnedCount > 0;

        //if (Input.GetButtonDown(Constants.InputStart))
        //{
        //    var to = GetAllTanks(Team.Green);
        //    var from = GetAllTanks(Team.Red);

        //    Aim(from, to);

            
            
        //}
    }
    
    //public void Aim(Tank_MB[] from, Tank_MB[] to)
    //{
    //    var radars = new (Tank_MB origin, Distance<Tank_MB>[] closestEnemies)[from.Length];

    //    Parallel.ForEach(from, (item, state, index) =>
    //    {
    //        var enemies = item.Attack.ClosestEnemies(to);

    //        radars[index] = (item, enemies);
    //    });

    //    //Obtendo um inimigo que não tenha nada entre o tanque e ele
    //    for (int i = 0; i < radars.Length; i++)
    //    {
    //        var bufferSize = radars[i].origin.Attack.Precision;
    //        var enemies = radars[i].closestEnemies;
    //        var origin = radars[i].origin;

    //        for (int j = 0; j < enemies.Length; j += bufferSize)
    //        {
    //            //Obtendo um array de tamanho Precision ou até o index máximo
    //            var tail = math.min(bufferSize, enemies.Length - j + bufferSize);
    //            var mostClosests = enemies.Skip(j).Take(tail).ToArray();

    //            Random.Shuffle(mostClosests);



    //            for (int k = 0; k < mostClosests.Length; k++)
    //            {
    //                //var target = mostClosests[k].Target.transform.position - origin.transform.position;
    //                //var sucess = Physics.Raycast(origin.transform.position, target, out var hitInfo, float.MaxValue, );
    //                //if (sucess)
    //                //{
    //                //    hitInfo.collider.CompareTag("")
    //                //    }
    //            }
    //        }


    //    }

    //}

    //struct SpawnTankJob : IJobParallelFor
    //{
    //    [ReadOnly]
    //    public NativeArray<SpawnData> SpawnData;
    //    public SpawnField_MB Field;
    //    public void Execute(int index)
    //    {
    //        var newTank = GameObject.Instantiate(SpawnData[index].TankPrefab, Field.transform);
    //        newTank.transform.position = SpawnData[index].Position;
    //        newTank.transform.eulerAngles = SpawnData[index].Rotation;

    //    }
    //}

    //public struct SpawnData
    //{
    //    public GameObject TankPrefab;
    //    public float3 Position;
    //    public float3 Rotation;
    //}


}

