﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct TankProperties : IComponentData
{
    public BlobAssetReference<TankStatsData> Blob;
    public Entity Model;
    public float3 AimTo;
    public float CurrentLife;
    public bool Locked;
    public Team Team;
}

//
public struct TankStatsData
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public BlobString Name;
    public int RadarAccuracy;
    public float RadarDelay;
    public float Delay => 1f / Cadence;
}