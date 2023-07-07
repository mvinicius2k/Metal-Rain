using System.Collections.Generic;

public class TanksProperty
{
    public List<SpawnField_MB> Spawns;
    public int DeadCount;
    public int SpawnedCount;

    public bool Empty => DeadCount >= SpawnedCount;

    public TanksProperty()
    {
        Spawns = new List<SpawnField_MB>();
    }
}

