using Unity.Entities;
using UnityEngine;

public class BulletMono : MonoBehaviour
{
    public TankStatsBase Stats;
    public float MaxDuration = 30f;
    public Vector3 ColliderSize;
    [Tooltip("Para obter o centro do objeto")]
    public Transform Center;

    private void OnDrawGizmosSelected()
    {
        if (Center == null)
            return;
        var bak = Gizmos.matrix;
        Gizmos.color = CustomColors.alpha2Red;
        Gizmos.matrix = Matrix4x4.TRS(Center.transform.position, transform.localRotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, ColliderSize);
        Gizmos.matrix = bak;


    }
}

public class BulletMonoBaker : Baker<BulletMono>
{
    public override void Bake(BulletMono authoring)
    {
        var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
        AddComponent(entity, new Bullet
        {
            Damage = authoring.Stats.Damage,
            Entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic),
            ColliderSize = authoring.ColliderSize,
            Center = GetEntity(authoring.Center.gameObject, TransformUsageFlags.Dynamic),
            Layer = new Unity.Physics.CollisionFilter
            {
                BelongsTo = (uint)Layer.Tank,
                CollidesWith = (uint)Layer.Tank,
            }
        }); ;

        AddComponent(entity, new Countdown
        {
            Value = authoring.MaxDuration
        });
    }
}

