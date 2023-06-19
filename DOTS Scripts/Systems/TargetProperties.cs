using Unity.Collections;
using Unity.Entities;

public partial struct RadarSystem
{
    public struct TargetProperties
    {
        public NativeArray<Entity> Models;
        public NativeArray<ArchetypeChunk> TankChunks;
        public int Count;

    }
}


