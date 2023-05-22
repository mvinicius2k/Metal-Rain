using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
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

    public static T GetRandom<T>(this T[] sources)
        {
            

            var clip = UnityEngine.Random.Range(0, sources.Length);
            return sources[clip];
        }

    public static NativeArray<T> MinN<T>(this in NativeArray<T> array, int n) where T : struct, IComparable<T>
    {
        var result = new NativeArray<T>(n, Allocator.Temp);


        
        for (int i = 0; i < array.Length; ++i)
        {
            for (int j = 0; j < result.Length; j++)
            {
                if(array[i].CompareTo(result[j]) > 0)
                {
                    result[i] = array[i];
                    break;
                }
            }
        }

        return result;
    }

}
