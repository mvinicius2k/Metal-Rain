using Unity.Entities;
using Unity.Mathematics;

public struct TankSpawner : IComponentData
{
    public float2 Start;
    public float2 End;
    public float2 BlockSize;
    public Team Team;
    /// <summary>
    /// Em radianos
    /// </summary>
    public float Orientation;
}


