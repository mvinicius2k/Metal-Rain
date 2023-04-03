using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct TankProperties : IComponentData
{
    public BlobAssetReference<TankStatsData> Blob;
    public Entity Prefab;
    public float3 AimTo;
    public float CurrentLife;
    public float Timer;
    public bool Locked;
}

public struct RedTeamTag : IComponentData { }
public struct GreenTeamTag : IComponentData { }

//
public struct TankStatsData
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public BlobString Name;
}