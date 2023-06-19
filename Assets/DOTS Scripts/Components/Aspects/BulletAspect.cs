using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct BulletAspect : IAspect
{
    public readonly Entity Entity;

    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRO<Bullet> Bullet;
    public readonly RefRW<Countdown> Countdown;

    public float3 ColliderSize => Bullet.ValueRO.ColliderSize;

}

