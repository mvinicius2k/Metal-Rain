﻿using Unity.Entities;

public readonly partial struct ApplyDamageAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRO<AliveTankTag> AliveTank;
    public readonly RefRW<TankAttack> Attack;
    private readonly RefRO<TankProperties> properties;
    public float Timer { get => Attack.ValueRO.ShootTimer; set => Attack.ValueRW.ShootTimer = value; }
    public Entity TargetEntity => Attack.ValueRO.Target;
    public TankProperties Properties => properties.ValueRO;
    public ref TankStatsData BaseProperties => ref properties.ValueRO.Blob.Value;

}



