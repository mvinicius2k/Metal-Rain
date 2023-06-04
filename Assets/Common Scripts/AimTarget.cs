using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Distance<T> where T : MonoBehaviour
{
    public T Target;
    public float Value;
}

public struct DistanceComparerMB<T> : IComparer<Distance<T>> where T : MonoBehaviour
{
    public int Compare(Distance<T> x, Distance<T> y)
    {
        return x.Value.CompareTo(y.Value);
    }
}



