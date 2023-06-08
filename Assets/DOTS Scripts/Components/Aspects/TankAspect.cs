using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.VersionControl;
using UnityEngine;

public readonly partial struct TankAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRW<TankProperties> Properties;
    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRW<LocalToWorld> LocalToWorld;
    private readonly EnabledRefRO<AliveTankTag> alive;
    //
    [Optional]
    public readonly RefRO<StandbyTankTag> standbyTank;
    [Optional]
    public readonly RefRO<GreenTeamTag> greenTeamTag;
    [Optional]
    public readonly RefRO<RedTeamTag> redTeamTag;
    [Optional]//
    public readonly RefRW<TankAttack> Attack;
    public float RechargeTime => Properties.ValueRO.Blob.Value.Delay;
    public float3 Position => LocalTransform.ValueRO.Position;
    public Team Team => greenTeamTag.IsValid ? Team.Green : Team.Red;
    public Entity ModelEntity => Properties.ValueRO.Model;
    public int RadarAccuracy => Properties.ValueRO.Blob.Value.RadarAccuracy;
    public float RadarDelay => Properties.ValueRO.Blob.Value.RadarDelay;
    public bool AimLocked => !standbyTank.IsValid;

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
        var res = target - position;
        var normalized = math.normalize(res.ToXZ());
        var radians = math.atan2(normalized.x, normalized.y) - math.PI/2f;
        return radians;
        
    }

    public void SetAimTo(float3 target)
    {
        //Debug.Log($"Setando mira para {target}");
        var yAngle = GetYRotation(target);
        
        LocalTransform.ValueRW.Rotation = quaternion.EulerXYZ(0f, yAngle, 0f);
        
    }
}
