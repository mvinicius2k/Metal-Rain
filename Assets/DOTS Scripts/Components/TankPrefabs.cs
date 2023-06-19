using Unity.Entities;

public struct TankPrefabs : IBufferElementData
{
    public TankKind Kind;
    public Entity Prefab;

}
