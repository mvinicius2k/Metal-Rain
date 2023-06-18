using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public readonly partial struct SpawnSpawnFieldsAspect : IAspect
{
    public readonly Entity Entity;
    public readonly RefRO<StartSpawn> StartSpawn;
    public readonly DynamicBuffer<StartupPrefabs> StartupPrefabs;
}
