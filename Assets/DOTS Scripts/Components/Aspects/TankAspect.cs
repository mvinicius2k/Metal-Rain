﻿using System;
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

        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Em radianos</returns>
    public float GetYRotation(float3 target)
    {
        Properties.ValueRW.AimTo = target;
        Properties.ValueRW.Locked = true;

        var position = LocalToWorld.ValueRO.Position;
        var res = position - target;
        var normalized = math.normalize(res.ToXZ());
        var radians = math.atan2(normalized.x, normalized.y);
        //        LocalTransform.ValueRW.RotateY(radians);
        //return radians * (180f / math.PI);
        return radians;
        
    }
}
