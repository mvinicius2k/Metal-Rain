using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class Extensions
{
    #region Mono
    public static Vector2 ToXZ(this Vector3 vector)
        => new Vector2(vector.x, vector.z);
    public static void Shuffle<T>(this Unity.Mathematics.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.NextInt(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
    public static void Shuffle<T>(this Unity.Mathematics.Random rng, List<T> array)
    {
        int n = array.Count;
        while (n > 1)
        {
            int k = rng.NextInt(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
    public static T GetRandom<T>(this T[] sources)
    {
        var clip = UnityEngine.Random.Range(0, sources.Length);
        return sources[clip];
    }

    #endregion

    public static void Shuffle<T>(this Unity.Mathematics.Random rng, ref NativeArray<T> array) where T : struct
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.NextInt(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static void Shuffle<T>(this Unity.Mathematics.Random rng, ref NativeList<T> array) where T : unmanaged
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.NextInt(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static float2 ToXZ(this float3 vector)
        => new float2(vector.x, vector.z);

    //public static float GetDistanceOf(this in Unity.Physics.RaycastHit hit, float3 origin)
    //    => math.distance(hit., origin);

    [Obsolete]
    public static NativeArray<T> MinN<T>(this in NativeArray<T> array, int n) where T : struct, IComparable<T>
    {
        var result = new NativeArray<T>(n, Allocator.Temp);



        for (int i = 0; i < array.Length; ++i)
        {
            for (int j = 0; j < result.Length; j++)
            {
                if (array[i].CompareTo(result[j]) > 0)
                {
                    result[i] = array[i];
                    break;
                }
            }
        }

        return result;
    }


}
