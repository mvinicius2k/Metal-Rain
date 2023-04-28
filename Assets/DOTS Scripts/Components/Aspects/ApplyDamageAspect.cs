using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

public readonly partial struct ApplyDamageAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRW<TankAttack> TankAim;
    private readonly RefRO<TankProperties> properties;
    private readonly RefRW<AttackDelayTimer> timer;

    public float Timer { get => timer.ValueRO.Value; set => timer.ValueRW.Value = value; }
    public Entity TargetEntity => TankAim.ValueRO.Target;
    public TankProperties Properties => properties.ValueRO;
    public ref TankStatsData BaseProperties => ref properties.ValueRO.Blob.Value;
    

}



