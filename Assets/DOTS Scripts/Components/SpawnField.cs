using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct SpawnField : IComponentData
{
    public Entity Prefab;
}
