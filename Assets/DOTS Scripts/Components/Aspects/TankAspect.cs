using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct TankAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRW<TankProperties> Properties;
    public readonly RefRW<LocalTransform> LocalTransform;
    public readonly RefRO<AliveTankTag> alive;
    public readonly DynamicBuffer<Damage> Damage;
    public readonly EnabledRefRW<StandbyTankTag> standbyTank;
    [Optional]
    public readonly RefRO<GreenTeamTag> greenTeamTag;
    [Optional]
    public readonly RefRO<RedTeamTag> redTeamTag;
    [Optional]//
    public readonly RefRW<TankAttack> Attack;
    public float RechargeTime => Properties.ValueRO.Blob.Value.Delay;
    public Team Team => greenTeamTag.IsValid ? Team.Green : Team.Red;
    public Entity ModelEntity => Properties.ValueRO.Model;
    public int RadarAccuracy => Properties.ValueRO.Blob.Value.RadarAccuracy;
    public float RadarDelay => Properties.ValueRO.Blob.Value.RadarDelay;
    public bool IsFree => standbyTank.ValueRO;
    public Entity FirePoint => Properties.ValueRO.FirePoint;

    public float3 GetWorldPosition(in ComponentLookup<LocalToWorld> lookup)
    {
        return lookup.GetRefRO(Properties.ValueRO.Center).ValueRO.Position;
    }

    public float GetYRotation(float3 target, in ComponentLookup<LocalToWorld> lookup)
    {
        Properties.ValueRW.AimTo = target;

        var position = GetWorldPosition(in lookup);
        var res = target - position;
        var normalized = math.normalize(res.ToXZ());
        var radians = math.atan2(normalized.x, normalized.y) - math.PI / 2f;
        return radians;

    }

    public void SetAimTo(float3 target, in ComponentLookup<LocalToWorld> lookup)
    {
        //Debug.Log($"Setando mira para {target}");
        var yAngle = GetYRotation(target, in lookup);

        LocalTransform.ValueRW.Rotation = quaternion.EulerXYZ(0f, yAngle, 0f);


    }
}
