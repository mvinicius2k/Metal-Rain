using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct RedTankAspect : IAspect
{
    public readonly Entity Entity;

    private readonly RefRO<RedTeamTag> redTeamTag;
    private readonly RefRO<AliveTankTag> aliveTag;
    public readonly RefRO<LocalTransform> LocalTransform;

    public float3 WorldPosition => LocalTransform.ValueRO.Position;
}
