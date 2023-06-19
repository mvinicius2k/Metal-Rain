using UnityEngine;

public class Bullet_MB : MonoBehaviour
{

    public LayerMask CollidesWith;
    public Vector3 ColliderSize;
    public float Speed = 15f;
    public float MaxDuration = 10f;
    public Vector3 ColliderOffset;

    public bool Stopped => !gameObject.activeSelf;

    private float damage;
    private Weapon_MB weapon;



    private void Update()
    {
        if (Stopped)
            return;

        transform.Translate(Speed * Time.deltaTime * transform.up, Space.World);
    }

    private void Start()
    {
        if (StartParams.Instance != null)
            Speed = StartParams.Instance.StartModel.BulletSpeed;
    }

    private void FixedUpdate()
    {
        if (Stopped)
            return;

        if (MaxDuration <= 0f)
        {
            weapon.BulletPool.Release(gameObject);
            return;

        }
        MaxDuration -= Time.fixedDeltaTime;

        Collider[] colliders = Physics.OverlapBox(transform.position + ColliderOffset, ColliderSize / 2f, transform.rotation, CollidesWith);
        if (colliders.Length > 0)
        {
            var entity = colliders[0].GetComponentInParent<Tank_MB>();
            if (entity != null)
            {
                entity.DigestDamage(damage);
                weapon.BulletPool.Release(gameObject);
            }

        }



    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CustomColors.alphaRed;
        var bak = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.localRotation, Vector3.one);
        Gizmos.DrawCube(transform.position + ColliderOffset, ColliderSize);
        Gizmos.matrix = bak;
    }

    public void Initialize(Weapon_MB weapon)
    {
        this.weapon = weapon;
        damage = weapon.Data.Damage;
    }
}

