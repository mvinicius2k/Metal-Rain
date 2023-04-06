using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Aim_MB : MonoBehaviour
{
    public SpawnField_MB OwnField;
    public KeyCode KeyToAim;
    public bool Locked;

    private Tank_MB target;
    public Tank_MB Target => target;


    private void Awake()
    {
        OwnField = GetComponentInParent<SpawnField_MB>();

    }


    private void Update()
    {

        if (Input.GetKeyDown(KeyToAim))
        {
            var enemyTanks = OwnField.EnemyField.Tanks;
            var random = Random.Range(0, enemyTanks.Count);
            var tankToAim = enemyTanks[random];
            transform.LookAt(tankToAim.transform);
            Locked = true;

            target = tankToAim.GetComponent<Tank_MB>();
        }
    }



}

