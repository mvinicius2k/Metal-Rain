using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(menuName = "MetalRain/TankStats")]
public class TankStats_SO : ScriptableObject
{
    public float MaxLife;
    public float Damage;
    public float Cadence;
    public string Name;
    public float Delay => 1f / Cadence;
}
