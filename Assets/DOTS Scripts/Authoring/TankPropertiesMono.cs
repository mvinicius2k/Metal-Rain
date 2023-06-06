using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TankPropertiesMono : MonoBehaviour
{
    public TankStatsBase Stats;
    public GameObject Model;
    
}

public class TankStatsBlobAssetBaker : Baker<TankPropertiesMono>
{
    public override void Bake(TankPropertiesMono authoring)
    {

        var builder = new BlobBuilder(Unity.Collections.Allocator.Temp);
        ref TankStatsData data = ref builder.ConstructRoot<TankStatsData>();

        data.MaxLife = authoring.Stats.MaxLife;
        data.Damage = authoring.Stats.Damage;
        data.Cadence = authoring.Stats.Cadence;
        builder.AllocateString(ref data.Name, authoring.Stats.Name);

        var reference = builder.CreateBlobAssetReference<TankStatsData>(Unity.Collections.Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset(ref reference, out _);
        var entity = GetEntity(TransformUsageFlags.None);


        AddComponent(entity, new TankProperties
        {
            Blob = reference,
            Model = GetEntity(authoring.Model, TransformUsageFlags.Dynamic),
            CurrentLife = data.MaxLife,
            AimTo = math.forward()
        });
        AddSharedComponent(entity, new TankRandom
        {
            Value = new Unity.Mathematics.Random(50)
        });
        AddComponent(entity, new TankAttack());
        SetComponentEnabled<TankAttack>(entity, false);
        AddComponent<StandbyTankTag>(entity);
        AddBuffer<Damage>(entity);
        //AddBuffer<TargetedTank>(entity);
        //SetComponentEnabled<TankAttack>(entity, true);
        //AddComponent<CleanupTank>(entity);


    }
}




