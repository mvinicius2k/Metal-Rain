using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class PrefabsMono : MonoBehaviour
{
    public TankKind_Prefab[] TankKind_Prefab;
}

public class PrefabsMonoBaker : Baker<PrefabsMono>
{
    public override void Bake(PrefabsMono authoring)
    {
        var entity = GetEntity(authoring,TransformUsageFlags.WorldSpace);
        AddBuffer<StartupPrefabs>(entity);

        for (int i = 0; i < authoring.TankKind_Prefab.Length; i++)
        {
            AppendToBuffer(entity, new StartupPrefabs
            {
                Kind = authoring.TankKind_Prefab[i].Kind,
                Prefab = GetEntity(authoring.TankKind_Prefab[i].Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

