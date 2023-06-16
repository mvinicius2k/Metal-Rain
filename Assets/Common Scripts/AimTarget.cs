using System.Collections.Generic;
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



