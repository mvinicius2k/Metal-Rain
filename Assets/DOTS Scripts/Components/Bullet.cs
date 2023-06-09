﻿using Unity.Entities;
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
