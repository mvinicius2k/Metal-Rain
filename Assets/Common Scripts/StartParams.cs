using System;
using System.IO;
using Unity.Serialization.Json;
using UnityEngine;

[Serializable]
public class SpawnConfigDto
{
    public Vector3 WorldPosition;
    public Vector2 BlockSize, Start, End;
    public SpawnRateDto[] SpawnRates;
    public float Orientation;
    public Team Team;
    public int RandomSeed;
}
[Serializable]
public class SpawnRateDto
{
    public int Weight;
    public int Kind;
}

public class StartParams : MonoBehaviour
{
    public string DefaultLocation => Path.Combine(Application.persistentDataPath, "Params.json");

    public TankKind_Prefab[] TankKind_Prefabs;

    private static StartParams instance;
    public static StartParams Instance => instance;

    private SpawnConfigDto[] spawnConfigs;
    private string JsonLocation;
    private bool isValid;

    public bool IsValid => isValid;
    public SpawnConfigDto[] SpawnConfigs => spawnConfigs;

    public GameObject GetTankPrefab(TankKind kind)
    {
        for (int i = 0; i < TankKind_Prefabs.Length; i++)
        {
            if (kind == TankKind_Prefabs[i].Kind)
                return TankKind_Prefabs[i].Prefab;

        }

        Debug.LogError("Impossível inferir o prefab de tanque " + kind + " em " + TankKind_Prefabs);
        return null;
    }

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

        try
        {
            //var paramsStr = Environment.GetCommandLineArgs();
            //if (paramsStr.Length == 0)
            //{
            //    JsonLocation = DefaultLocation;
            //    Debug.Log($"Usando local de parâmetros padrão: {JsonLocation}");
            //}
            //else
            //    JsonLocation = paramsStr[0];

            JsonLocation = DefaultLocation;

            if (!File.Exists(JsonLocation))
            {
                Debug.LogWarning($"Não há arquivo de inicialização em {JsonLocation}");
                return;
            }

            var jsonText = File.ReadAllText(JsonLocation);
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                Debug.LogWarning($"{JsonLocation} é inválido");
                return;
            }

            spawnConfigs = JsonSerialization.FromJson<SpawnConfigDto[]>(jsonText);
            isValid = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            isValid = false;
        }

    }

    private void Start()
    {



    }

}
