using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Todos os tanques 
/// </summary>
public struct ExplorationMove : ISharedComponentData
{
    public float Speed;
    public float3 Target;


}


