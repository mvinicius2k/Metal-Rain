using System.Collections.Generic;
using UnityEngine;

public class SpawnFieldManager_MB : MonoBehaviour
{


    public Dictionary<Team, TanksProperty> TanksProperties;

    public Unity.Mathematics.Random Random;
    public GameObject SpawnFields;
    public bool EndGame;
    public bool Started;

    private static SpawnFieldManager_MB instance;
    public static SpawnFieldManager_MB Instance => instance;




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

    private void Start()
    {
        if (!StartParams.Instance.IsValid)
            return;

        foreach (var spawn in StartParams.Instance.StartModel.SpawnConfig)
        {
            var spawnObj = Instantiate<GameObject>(new GameObject(), SpawnFields.transform);
            //spawnObj.name = $"{spawn.Team} spawner";
            var field = spawnObj.AddComponent<SpawnField_MB>();
            field.FromDto(spawn);
            TanksProperties[field.Team].Spawns.Add(field);

        }
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

