using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct BulletAspect : IAspect
{
    public readonly Entity Entity;
    
    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRW<Bullet> Bullet;
    public readonly RefRW<Countdown> Countdown;

    public float3 ColliderSize => Bullet.ValueRO.ColliderSize;
    
}

