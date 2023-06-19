using Unity.Entities;
using Unity.Mathematics;

public struct TankSpawnerRateBuffer : IBufferElementData
{
    public Entity Prefab;
    public TankKind Kind;
    public int Weight;

    /// <summary>
    /// Obtém a parte do todo <paramref name="totalWeight"/>
    /// </summary>
    /// <param name="size"></param>
    /// <param name="totalWeight"></param>
    /// <returns></returns>
    public int GetTotalFrom(float size, float totalWeight)
    {
        var rate = Weight / totalWeight;
        var result = math.ceil(size * rate);
        return (int)result;
    }

}

