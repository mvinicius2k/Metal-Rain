using Unity.Entities;

public struct TankStatsData
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public BlobString Name;
    public int RadarAccuracy;
    public float RadarDelay;

    /// <summary>
    /// Tempo entre disparos (em segundos)
    /// </summary>
    public float Delay => 1f / Cadence;
}