﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TankPropertiesMono : MonoBehaviour
{
    public TankStatsBase Stats;
    public GameObject Model;
    [Tooltip("Lugar onde as balas saem")]
    public Transform FirePoint;
    [Tooltip("Centro do tanque, o pivô geralmente está embaixo do tanque")]
    public Transform Center;
    public GameObject BulletPrefab;

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
        data.RadarAccuracy = authoring.Stats.RadarAcuraccy;
        data.RadarDelay = authoring.Stats.RadarDelay;
        builder.AllocateString(ref data.Name, authoring.Stats.Name);

        var reference = builder.CreateBlobAssetReference<TankStatsData>(Unity.Collections.Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset(ref reference, out _);
        var entity = GetEntity(TransformUsageFlags.None);

        var model = GetEntity(authoring.Model, TransformUsageFlags.Dynamic);
        AddComponent(entity, new TankProperties
        {
            Blob = reference,
            Model = model,
            BulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic),
            AimTo = math.forward(),
            FirePoint = GetEntity(authoring.FirePoint, TransformUsageFlags.Dynamic),
            Center = GetEntity(authoring.Center, TransformUsageFlags.Dynamic)
        });
        AddComponent(entity, new TankDefense
        {
            CurrentLife = data.MaxLife,
        });
        //AddSharedComponent(entity, new TankRandom
        //{
        //    Value = new Unity.Mathematics.Random(50)
        //});
        AddComponent(entity, new TankAttack());
        SetComponentEnabled<TankAttack>(entity, false);
        AddComponent<StandbyTankTag>(entity);
        AddComponent<AliveTankTag>(entity);
        AddBuffer<Damage>(entity);

    }
}




