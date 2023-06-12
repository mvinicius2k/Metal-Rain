using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class Bullet_MB : MonoBehaviour
{
    public LayerMask CollidesWith;
    public Vector3 ColliderSize;
    public float Speed = 5f;
    public float MaxDuration = 10f;
    public Vector3 ColliderOffset;


    public bool Stopped => !gameObject.activeSelf || colliding;

    private float damage;
    private bool colliding;
    private Weapon_MB weapon;


    private void Start()
    {
        //angle = Vector3.Angle(Vector3.right, transform.eulerAngles);
    }

    private void Update()
    {
        if (Stopped)
            return;

        transform.Translate(Vector3.right * Speed * Time.deltaTime, Space.Self);
    }

    private void FixedUpdate()
    {
        if (Stopped)
            return;

        if(MaxDuration <= 0f || colliding)
        {
            colliding = true;
            Destroy(gameObject);
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
            }

        }



    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CustomColors.alphaRed;
        Gizmos.DrawCube(transform.position + ColliderOffset, ColliderSize);
    }

    public void Initialize(Weapon_MB weapon)
    {
        this.weapon = weapon;
        damage = weapon.Data.Damage;
    }
}

