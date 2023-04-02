using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class TankStatsBlobAsset : MonoBehaviour
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public string Name;
    public GameObject Prefab;
}

public class TankStatsBlobAssetBaker : Baker<TankStatsBlobAsset>
{
    public override void Bake(TankStatsBlobAsset authoring)
    {

        var builder = new BlobBuilder(Unity.Collections.Allocator.Temp);
        ref TankStatsData data = ref builder.ConstructRoot<TankStatsData>();

        data.MaxLife = authoring.MaxLife;
        data.Damage = authoring.Damage;
        data.Cadence = authoring.Cadence;
        builder.AllocateString(ref data.Name, authoring.Name);

        var reference = builder.CreateBlobAssetReference<TankStatsData>(Unity.Collections.Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset(ref reference, out _);
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new TankStats
        {
            Blob = reference,
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
        });

        Debug.Log("Cozido");

    }
}




