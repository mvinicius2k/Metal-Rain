using System.Collections.Generic;
using UnityEngine;

public class TankAttack_MB : MonoBehaviour
{


    public int Precision = 3;
    public float RadarDelay = 1f;
    public Tank_MB Tank;
    public bool Start;
    public Weapon_MB Weapon;


    private Tank_MB targetedEnemy;
    private Unity.Mathematics.Random random;
    private float radarCount = 0f;
    private float attackCount;


    public Tank_MB EnemyTargeted { get => targetedEnemy; set => targetedEnemy = value; }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawns"></param>
    /// <returns>Uma lista de inimigos atingíveis ordenadas pela distância</returns>
    public List<Distance<Tank_MB>> ClosestEnemies(List<SpawnField_MB> spawns)
    {
        var distances = new List<Distance<Tank_MB>>();

        foreach (var spawn in spawns)
        {
            foreach (var enemy in spawn.Tanks)
            {
                var distance = Vector3.Distance(Tank.transform.position, enemy.transform.position);
                distances.Add(new Distance<Tank_MB>
                {
                    Target = enemy,
                    Value = distance
                });
            }

        }
        distances.Sort(new DistanceComparerMB<Tank_MB>());
        return distances;

    }





    private void Awake()
    {
        random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        radarCount = random.NextFloat(0f, RadarDelay);
        attackCount = random.NextFloat(0f, Tank.Stats.ShootDelay);
    }

    private void Update()
    {
        if (SpawnFieldManager_MB.Instance.EndGame)
            return;

        if (Input.GetButtonDown(Constants.InputStart))
        {
            Start = true;
        }

        if (!Start)
            return;

        if (radarCount > 0f)
            radarCount -= Time.deltaTime;

        if (attackCount > 0f)
            attackCount -= Time.deltaTime;

        if (targetedEnemy && attackCount <= 0f)
            Weapon.TryFire();

        if (targetedEnemy == null && radarCount <= 0f)
        {
            var enemy = FoundEnemy();
            if (enemy != null)
            {
                targetedEnemy = enemy;
                transform.right = enemy.transform.position - transform.position;
                Weapon.FirePoint.up = enemy.Center.position - Weapon.FirePoint.position;
                targetedEnemy.TargetedBy.Add(this);
                //Debug.DrawRay(Weapon.FirePoint.position, enemy.Center.position - Weapon.FirePoint.position, Tank.Team == Team.Green ? Color.green : Color.red, 2f);
            }

        }




    }




    private Tank_MB FoundEnemy()
    {
        var fields = SpawnFieldManager_MB.Instance.TanksProperties[Tank.SpawnField.EnemyTeam].Spawns;
        var enemies = ClosestEnemies(fields);
        if (enemies.Count == 0)
            return null;
        radarCount = RadarDelay;



        var enemiesToChoose = new List<Distance<Tank_MB>>(Precision);
        for (int i = 0; i < enemies.Count; i++)
        {
            var ray = new Ray(transform.position, enemies[i].Target.transform.position - transform.position);
            Physics.Raycast(ray, out var info, Mathf.Infinity, LayerMask.GetMask(Constants.LayerTank));
            if (info.collider != null && info.collider.CompareTag(Tank.SpawnField.EnemyTag))
                enemiesToChoose.Add(enemies[i]);

            if (enemiesToChoose.Count == enemiesToChoose.Capacity - 1)
                break;
        }


        if (enemiesToChoose.Count > 0)
        {
            var randomIndex = random.NextInt(0, enemiesToChoose.Count - 1);
            return enemiesToChoose[randomIndex].Target;
        }
        else
            return null;



    }


    public override int GetHashCode()
        => GetInstanceID();
}
