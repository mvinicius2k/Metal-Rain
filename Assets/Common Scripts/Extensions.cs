using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public static class Extensions
{
    #region Mono
    public static Vector2 ToXZ(this Vector3 vector)
        => new Vector2(vector.x, vector.z);
    #endregion
    public static float2 ToXZ(this  float3 vector)
        => new float2(vector.x, vector.z);
}
