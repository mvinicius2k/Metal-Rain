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
    private bool endgame;


    public Tank_MB EnemyTargeted { get => targetedEnemy; set => targetedEnemy = value; }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="spawns"></param>
    /// <returns>Uma lista de inimigos atingíveis ordenadas pela distância</returns>
    public List<Distance<Tank_MB>> ClosestEnemies(List<SpawnField_MB> spawns)
    {
        var distances = new List<Distance<Tank_MB>>();

        var index = 0;
        foreach (var spawn in spawns)
        {
            foreach (var enemy in spawn.Tanks)
            {
                var ray = new Ray(transform.position, enemy.transform.position - transform.position);
                var sucess = Physics.Raycast(ray, Mathf.Infinity, LayerMask.GetMask(Constants.LayerTank));
                if (sucess)
                {
                    var distance = Vector3.Distance(Tank.transform.position, enemy.transform.position);
                    distances.Add(new Distance<Tank_MB>
                    {
                        Target = enemy,
                        Value = distance
                    });

                    if (distances.Count == Precision)
                        goto Sort; //Goto sofre hate
                    
                }
                
            }
        }
        Sort:
        //for (int i = 0; i < distances.Length; i++)
        //{
        //    var distance = Vector3.Distance(Tank.transform.position, enemies[i].transform.position);
        //    distances[i] = new Distance<Tank_MB>
        //    {
        //        Target = enemies[i],
        //        Value = distance
        //    };

        //}


        distances.Sort(new DistanceComparerMB<Tank_MB>());
        return distances;


    }

    private void Awake()
    {
        random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        radarCount = random.NextFloat(0f, RadarDelay);
        attackCount = random.NextFloat(0f, Tank.Stats.Delay);
    }

    private void Update()
    {
        if (endgame)
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
            else
            {
                Debug.Log("Fim de jogo");
                endgame = true;
            }
            
        }




    }



    private void Attack(Tank_MB target)
    {
        target.CurrentLife -= Tank.Stats.Damage;
        attackCount = Tank.Stats.Delay;

        if (target.CurrentLife <= 0f)
        {
            Destroy(target.gameObject);
            target = null;
        }
    }

    private Tank_MB FoundEnemy()
    {
        
        var enemies = ClosestEnemies(SpawnFieldManager.Instance.Spawns[Tank.SpawnField.EnemyTeam]);
        if (enemies.Count == 0)
            return null;
        radarCount = RadarDelay;
        var tail = math.min(Precision, enemies.Count - 1);
        var randomIndex = random.NextInt(0, tail);
        return enemies[randomIndex].Target;
        //for (int i = 0; i < enemies.Count; i += Precision)
        //{
        //    var tail = math.min(i + Precision, enemies.Count - 1);

        //    var mostClosest = enemies.Skip(i).Take(tail).ToArray(); //Remover linq
        //    random.Shuffle(mostClosest);

        //    for (int j = 0; j < mostClosest.Length; j++)
        //    {
        //        var ray = new Ray(transform.position, mostClosest[j].Target.transform.position - transform.position);
        //        var sucess = Physics.Raycast(ray, float.MaxValue, LayerMask.GetMask(Constants.LayerTank));
        //        if (sucess)
        //        {
        //            return mostClosest[j].Target;
        //        }


        //    }

        //}

        return null;
    }


    public override int GetHashCode()
        => GetInstanceID();
}
