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
    public bool Locked;
    public Team Team;
}
public struct RedTeamTag : IComponentData 
{
    public bool ValueThatWillNeverBeUsed; //Bug? ComponentTag precisa de algum campo para ser verificavel se é válido ou nao dentro de aspect
}
public struct GreenTeamTag : IComponentData 
{
    public bool ValueThatWillNeverBeUsed;
}

//
public struct TankStatsData
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public BlobString Name;
    public float Delay => 1f / Cadence;
}