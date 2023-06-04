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

[CreateAssetMenu(menuName = "MetalRain/Balanced Tank Stats")]
public class BalancedTankStats : TankStatsBase
{

}

[CreateAssetMenu(menuName = "MetalRain/Hunter Tank Stats")]
public class HunterTankStats : TankStatsBase
{

}

[CreateAssetMenu(menuName = "MetalRain/Light Tank Stats")]
public class LightTankStats : TankStatsBase
{

}

[CreateAssetMenu(menuName = "MetalRain/Heavy Tank Stats")]
public class HeavyTankStats : TankStatsBase
{
    public float MissileSpeed;
    public float ExplosionRadius;
    public float ExplosionDamage;
}
