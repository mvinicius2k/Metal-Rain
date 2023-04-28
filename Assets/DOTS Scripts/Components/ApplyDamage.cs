using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct ApplyDamage : IBufferElementData
{
    public Entity From;
    public float Damage;
    public float Cadence;

}

public struct AttackDelayTimer : IComponentData
{
    public float Value;
}


