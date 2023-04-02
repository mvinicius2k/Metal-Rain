using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;



public struct TankStats : IComponentData
{
    public BlobAssetReference<TankStatsData> Blob;
    public Entity Prefab;
}


//
public struct TankStatsData
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public BlobString Name;
}