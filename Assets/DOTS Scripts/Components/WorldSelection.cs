using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct WorldSelection : IComponentData
{
    public RaycastInput Start, End;
    public bool Active;

}

public struct SelectionTile : IComponentData
{
    /// <summary>
    /// 1x1
    /// </summary>
    public Entity Tile;

}
