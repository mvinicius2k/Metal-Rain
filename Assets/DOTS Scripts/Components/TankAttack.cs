using Unity.Entities;

public struct TankAttack : IEnableableComponent, IComponentData
{
    public Entity Target;
    public float ShootTimer;
    public float RadarTimer;
}




public struct Damage : IBufferElementData
{
    public float Value;

}
