using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Dispõe os prefabs para fazer o bake para o mundo Entities. Seria dispensável num jogo real.
/// </summary>
public class PrefabsMono : MonoBehaviour
{
    [Tooltip("Vínculo entre o tipo de tanque o o prefab")]
    public TankKind_Prefab[] TankKind_Prefab;
    public GameObject SpawnFieldPrefab;
}

public class PrefabsMonoBaker : Baker<PrefabsMono>
{
    public override void Bake(PrefabsMono authoring)
    {
        var entity = GetEntity(authoring,TransformUsageFlags.WorldSpace);
        AddBuffer<TankPrefabs>(entity);
        AddComponent(entity, new SpawnField
        {
            Prefab = GetEntity(authoring.SpawnFieldPrefab, TransformUsageFlags.WorldSpace)
        });
        for (int i = 0; i < authoring.TankKind_Prefab.Length; i++)
        {
            AppendToBuffer(entity, new TankPrefabs
            {
                Kind = authoring.TankKind_Prefab[i].Kind,
                Prefab = GetEntity(authoring.TankKind_Prefab[i].Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

