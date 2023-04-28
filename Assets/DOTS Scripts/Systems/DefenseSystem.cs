using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public partial struct DefenseSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);


    }
}

public partial struct DigestDamageJob : IJobEntity
{
    public EntityCommandBuffer Ecb;
    public void Execute(AttackedTankAspect attackedTank)
    {
        for (int i = 0; i < attackedTank.DamageBuffer.Length; i++)
        {
            attackedTank.Life -= attackedTank.DamageBuffer[i].Value;
        }

        if (attackedTank.Life <= 0)
        {
            for (int i = 0; i < attackedTank.DamageBuffer.Length; i++)
            {
                var tankToFree = attackedTank.DamageBuffer[i].Source;
                Ecb.SetComponent(tankToFree, new TankAttack
                {
                    Target = Entity.Null
                });
                Ecb.SetComponentEnabled<TankAttack>(tankToFree, false);
            }
            Ecb.SetComponentEnabled<AliveTankTag>(attackedTank.Entity, false);

            attackedTank.DamageBuffer.Clear();
            Ecb.DestroyEntity(attackedTank.Entity);
            return;

        }

        attackedTank.DamageBuffer.Clear();


    }
}
