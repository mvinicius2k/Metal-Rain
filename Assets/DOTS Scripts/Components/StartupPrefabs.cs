﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct StartSpawn : IComponentData
{
    public Entity Prefab;
}

public struct StartupPrefabs : IBufferElementData
{
    public TankKind Kind;
    public Entity Prefab;

}
