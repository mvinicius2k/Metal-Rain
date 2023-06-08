using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;

public class TankAttack_MB : MonoBehaviour
{


    public int Precision = 3;
    public float RadarDelay = 1f;
    public Tank_MB Tank;
    public bool Start;

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
    if (SpawnFieldManager.Instance.EndGame)
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
        Attack(targetedEnemy);

    if (targetedEnemy == null && radarCount <= 0f)
    {
        var enemy = FoundEnemy();
        if (enemy != null)
        {
            targetedEnemy = enemy;
            transform.right = enemy.transform.position - transform.position;
            targetedEnemy.TargetedBy.Add(this);
        }

    }




}



private void Attack(Tank_MB target)
{
    target.CurrentLife -= Tank.Stats.Damage;
    attackCount = Tank.Stats.ShootDelay;

    if (target.CurrentLife <= 0f)
    {
        Destroy(target.gameObject);
        target = null;
    }
}

private Tank_MB FoundEnemy()
{
    var fields = SpawnFieldManager.Instance.TanksProperties[Tank.SpawnField.EnemyTeam].Spawns;
    var enemies = ClosestEnemies(fields);
    if (enemies.Count == 0)
        return null;
    radarCount = RadarDelay;

    for (int i = 0; i < enemies.Count; i += Precision)
    {
        var tail = math.min(i + Precision, enemies.Count - 1);

        var mostClosest = enemies.Skip(i).Take(tail).ToArray(); //Remover linq
        random.Shuffle(mostClosest);

        for (int j = 0; j < mostClosest.Length; j++)
        {
            var ray = new Ray(transform.position, mostClosest[j].Target.transform.position - transform.position);
            Physics.Raycast(ray, out var info, Mathf.Infinity, LayerMask.GetMask(Constants.LayerTank));
            if (info.collider != null && info.collider.CompareTag(Tank.SpawnField.EnemyTag))
            {
                return mostClosest[j].Target;
            }


        }

    }

    return null;

}


public override int GetHashCode()
    => GetInstanceID();
}
