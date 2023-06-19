using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Estrutura para armazenar os tanques inimigos suas distâncias
/// </summary>
public struct AimTarget
{
    public Entity Entity;
    public float3 Position;
    public float Distance;

}
