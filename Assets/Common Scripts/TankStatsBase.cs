using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public abstract class TankStatsBase : ScriptableObject
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public string Name;
    public float Delay => 1f / Cadence;
}
