using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
