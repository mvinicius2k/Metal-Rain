using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct TankSpawner : IComponentData
{
    public float2 Start;
    public float2 End;
    public float2 BlockSize;
    //public Entity ChosenTank;
    public Team Team;
    /// <summary>
    /// Em radianos
    /// </summary>
    public float Orientation;
}


