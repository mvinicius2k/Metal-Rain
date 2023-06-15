using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public struct TankSpawnerRateBuffer : IBufferElementData
{
    public Entity Prefab;
    public TankKind Kind;
    public int Weight;

    public int GetTotalFrom(float size, float totalWeight)
    {
        var rate = Weight / totalWeight;
        var result = math.ceil(size * rate);
        return (int) result;
    }
   
}

