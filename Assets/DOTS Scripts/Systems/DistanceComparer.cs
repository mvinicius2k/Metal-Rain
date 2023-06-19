using System.Collections.Generic;

/// <summary>
/// Utilitário para se usar como Comparer de distância
/// </summary>
public struct DistanceComparer : IComparer<AimTarget>
{
    public int Compare(AimTarget x, AimTarget y)
    => x.Distance.CompareTo(y.Distance);
}
