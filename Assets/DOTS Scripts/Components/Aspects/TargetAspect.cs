﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct TargetAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRO<LocalTransform> Transform;

    public float3 Position => Transform.ValueRO.Position;


}
