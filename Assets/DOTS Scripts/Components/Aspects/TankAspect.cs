using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct TankAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRW<TankProperties> Properties;
    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRW<LocalToWorld> LocalToWorld;
    //[Optional]
    //public readonly RefRO<GreenTeamTag> GreenTeamTag;
    //[Optional]
    //public readonly RefRO<RedTeamTag> RedTeamTag;
    public bool AimLocked
    { 
        get => Properties.ValueRO.Locked;
        set => Properties.ValueRW.Locked = value; 
    }
        

    public void SetAimTo(float3 target)
    {
        Properties.ValueRW.AimTo = target;
        Properties.ValueRW.Locked = true;

        var frontTank = LocalTransform.ValueRO.Forward();
        var res = frontTank - target;
        var normalized = math.normalize(res.ToXZ());
        var radians = math.atan2(normalized.x, normalized.y);
        LocalTransform.ValueRW.RotateY(radians);
    }
}
