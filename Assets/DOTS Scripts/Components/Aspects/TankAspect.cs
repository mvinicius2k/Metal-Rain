using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.VersionControl;

public readonly partial struct TankAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRW<TankProperties> Properties;
    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRW<LocalToWorld> LocalToWorld;



    public readonly EnabledRefRO<TankAimFreeTag> TankAimLockedTag;
    public readonly EnabledRefRO<AliveTankTag> TankAlive;
    [Optional]
    public readonly RefRO<GreenTeamTag> GreenTeamTag;
    [Optional]
    public readonly RefRO<RedTeamTag> RedTeamTag;
    public bool AimLocked => TankAimLockedTag.ValueRO;
    public float RechargeTime => Properties.ValueRO.Blob.Value.Delay;

    public Team Team => GreenTeamTag.IsValid ? Team.Green : Team.Red;

        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Em radianos</returns>
    public float GetYRotation(float3 target)
    {
        Properties.ValueRW.AimTo = target;
        Properties.ValueRW.Locked = true;

        var position = LocalToWorld.ValueRO.Position;
        var res = position - target;
        var normalized = math.normalize(res.ToXZ());
        var radians = math.atan2(normalized.x, normalized.y);
        return radians;
        
    }

    public void SetAimTo(float3 target)
    {
        //Debug.Log($"Setando mira para {target}");
        var yAngle = GetYRotation(target);
        LocalTransform.ValueRW.Rotation = quaternion.EulerXYZ(0f, yAngle, 0f);
        
    }
}
