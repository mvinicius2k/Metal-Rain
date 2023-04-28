using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct TankAttack : IEnableableComponent, IComponentData
{
    public Entity Target;
    
}




public struct Damage : IBufferElementData
{
    public float Value;
    public Entity Source;
}
