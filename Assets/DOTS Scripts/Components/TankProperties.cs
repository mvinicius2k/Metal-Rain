using Unity.Entities;
using Unity.Mathematics;

public struct TankProperties : IComponentData
{
    public BlobAssetReference<TankStatsData> Blob;
    public Entity Model;
    public float3 AimTo;
    public Entity FirePoint;
    public Entity Center;
    public Entity BulletPrefab;
}
