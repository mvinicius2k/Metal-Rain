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
    public readonly DynamicBuffer<ApplyDamage> ApplyDamage;
    public readonly RefRW<TankProperties> Properties;
    private readonly RefRW<ApplyDamageTimer> timer;

    public float Timer { get => timer.ValueRO.Value; set => timer.ValueRW.Value = value; }
    public void RestartTimer()
    {
        Timer = Properties.ValueRO.Blob.Value.Delay;
    }

}



