using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Tank_MB : MonoBehaviour
{
    public TankStats_SO Stats;
    public Aim_MB Aim;
    public float CurrentLife;

    private float timerShot;
    private Stopwatch stopwatch;

    private void Awake()
    {
        
    }
    private void Start()
    {
        
        CurrentLife = Stats.MaxLife;
        timerShot = Random.Range(0f, Stats.Delay); //Pequeno random para os tanque não atirarem todos sempre ao mesmo tempo
    }

    private void Update()
    {
        if (!Aim.Locked || Aim.Target == null)
            return;

        if(timerShot <= 0f) //hora de atirar
        {
            Aim.Target.CurrentLife -= Stats.Damage;

            timerShot = Stats.Delay;
            if (Aim.Target.CurrentLife <= 0)
            {
                
                Destroy(Aim.Target.gameObject);
                
            }
            
        }
        else
            timerShot -= Time.deltaTime;


    }
}
