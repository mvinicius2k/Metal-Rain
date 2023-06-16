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
    public GameObject BulletPrefab;
    public float ShootDelay => 1f / Cadence;
}
