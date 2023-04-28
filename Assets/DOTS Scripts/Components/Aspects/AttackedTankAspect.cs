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
    public readonly DynamicBuffer<Damage> DamageBuffer;

    public float Life { get => Properties.ValueRO.CurrentLife; set => Properties.ValueRW.CurrentLife = value; }

}
