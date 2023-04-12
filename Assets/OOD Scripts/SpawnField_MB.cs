using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class SpawnField_MB : MonoBehaviour
{

    public Vector2 BlockSize, StartAt, EndAt;
    public GameObject ChosenTank;
    public Color GizmosColor;
    public KeyCode KeyToSpawn;
    public SpawnField_MB EnemyField;

    private List<GameObject> tanks;

    private int xLimit, zLimit;

    public List<GameObject> Tanks { get => tanks; }

    private void Start()
    {
        xLimit = Mathf.RoundToInt(Mathf.Abs((StartAt.x - EndAt.x)) / BlockSize.x);
        zLimit = Mathf.RoundToInt(Mathf.Abs((StartAt.y - EndAt.y)) / BlockSize.y);
        tanks = new List<GameObject>(xLimit * zLimit);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyToSpawn))
        {
            for (int i = 0; i < xLimit; i++)
            {
                for (int z = 0; z < zLimit; z++)
                {
                    var newTank = Instantiate(ChosenTank, transform);
                    newTank.transform.position = new Vector3
                    {
                        x = transform.position.x + i * BlockSize.x,
                        y = transform.position.y,
                        z = transform.position.z + z * BlockSize.y,
                    };
                    
                    tanks.Add(newTank);
                }
            }

            StaticBatchingUtility.Combine(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmosColor;
        var center = new Vector3
        {
            x = transform.position.x + ((StartAt.x + EndAt.x) / 2f),
            y = transform.position.y,
            z = transform.position.z + ((StartAt.y + EndAt.y) / 2f)
        };

        var size = new Vector3
        {
            x = Mathf.Abs(StartAt.x - EndAt.x),
            y = 0.5f,
            z = Mathf.Abs(StartAt.y - EndAt.y)
        };

        //
        Gizmos.DrawCube(center, size);
    }




}
