using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct Bullet : IComponentData
{
    public float Damage;
    public Entity Entity;
    public float3 ColliderSize;
    public CollisionFilter Layer;
    public Entity Center;
}

public struct Countdown : IComponentData
{
    public float Value;
}
