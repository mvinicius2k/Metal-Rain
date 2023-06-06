using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

//public readonly partial struct TankModelAspect : IAspect
//{
//    public readonly Entity Entity;
//    public readonly RefRW<LocalTransform> LocalTransform;
//    private readonly RefRO<AliveTankTag> alive;

 

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <returns><see langword="false"/> se o componente <see cref="LookAt"/> não estiver atribuído</returns>
//    public bool TryAim()
//    {
//        if (!LookAt.IsValid)
//            return false;
//        var position = LocalTransform.ValueRO.Position;
//        var res = LookAt.ValueRO.Target - position;
//        var normalized = math.normalize(res.ToXZ());
//        var radians = math.atan2(normalized.x, normalized.y);

//        LocalTransform.ValueRW.Rotation = quaternion.EulerXYZ(0f, radians, 0f);
//        return true;
//    }
//}