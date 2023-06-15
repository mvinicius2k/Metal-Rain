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


    public Dictionary<Team, TanksProperty> TanksProperties;

    public Unity.Mathematics.Random Random;
    public GameObject SpawnFields;
    public bool EndGame;
    public bool Started;

    private static SpawnFieldManager instance;
    public static SpawnFieldManager Instance => instance;




    private void Awake()
    {
        if (instance == null)
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


    }

}

