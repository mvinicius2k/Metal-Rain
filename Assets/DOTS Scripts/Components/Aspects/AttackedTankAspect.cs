using Unity.Entities;


public readonly partial struct AttackedTankAspect : IAspect
{
    public readonly Entity Entity;

    public readonly RefRW<TankProperties> Properties;
    public readonly RefRW<TankAttack> Attack;
    public readonly DynamicBuffer<Damage> DamageBuffer;
    public readonly RefRW<TankDefense> Defense;
    public float Life { get => Defense.ValueRO.CurrentLife; set => Defense.ValueRW.CurrentLife = value; }



}
