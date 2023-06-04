using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;

public class Tank_MB : MonoBehaviour
{
    public TankStatsBase Stats;
    //public Aim_MB Aim;
    public TankAttack_MB Attack;
    public float CurrentLife;
    public GameObject Model;
    public UnityEvent<Tank_MB> OnDead;
    public Team Team => SpawnField.Team;
    

    public HashSet<TankAttack_MB> TargetedBy { get => targetedBy;}
    public SpawnField_MB SpawnField { get => spawnField; set => spawnField = value; }

    private SpawnField_MB spawnField;
    private Team enemyTeam;
    private HashSet<TankAttack_MB> targetedBy;


    private void Awake()
    {
        CurrentLife = Stats.MaxLife;
        targetedBy = new();
        
    }
        private void OnDestroy()
    {
        
        foreach (var attack in targetedBy)
        {
            attack.EnemyTargeted = null;
        }
        spawnField.Tanks.Remove(this);
        OnDead.Invoke(this);
    }

  

    public void Dependencies(SpawnField_MB field)
    {
        spawnField = field;
        Model.tag = field.Team == Team.Green ? Constants.TagGreenTeam : Constants.TagRedTeam;

    }
}
