using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TankSpawnerMono : MonoBehaviour
{
    
    public Vector2 BlockSize, Start, End;
    public GameObject ChosenTank;
    public Color GizmosColor;
    public Team Team;

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmosColor;
        var center = new Vector3
        {
            x = transform.position.x + ((Start.x + End.x) / 2f),
            y = transform.position.y,
            z = transform.position.z + ((Start.y + End.y) / 2f)
        };

        var size = new Vector3
        {
            x = Mathf.Abs(Start.x - End.x),
            y = transform.position.y,
            z = Mathf.Abs(Start.y - End.y)
        };
        
        //
        Gizmos.DrawCube(center, size);
    }

}

public class TankSpawnerMonoBaker : Baker<TankSpawnerMono>
{
    public override void Bake(TankSpawnerMono authoring)
    {
        var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
        AddComponent(entity, new TankSpawner
        {
            BlockSize = authoring.BlockSize,
            Start = authoring.Start,
            End = authoring.End,
            ChosenTank = GetEntity(authoring.ChosenTank, TransformUsageFlags.Dynamic),
            Team = authoring.Team,
        });
        
        //if(authoring.Team == Team.Green)
        //    AddComponent(entity, new GreenTeamTag());
        //else
        //    AddComponent(entity, new RedTeamTag());
        Debug.Log("Cozido");

    }
}

