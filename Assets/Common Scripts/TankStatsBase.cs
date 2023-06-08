using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public abstract class TankStatsBase : ScriptableObject
{
    public float MaxLife;
    public float Damage;
    [Tooltip("Tiros por segundo")]
    public float Cadence;
    public string Name;
    public int RadarAcuraccy;
    public float RadarDelay;
    public float ShootDelay => 1f / Cadence;
}
