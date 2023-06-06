using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

//public partial struct InitializeTankSystem : ISystem
//{
//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        var singleton = SystemAPI.GetSingleton<BeginVariableRateSimulationEntityCommandBufferSystem.Singleton>();
//        var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

//        foreach ((var prepare, TankProperties tank , var entity) in SystemAPI.Query<PrepareTank, TankProperties>().WithEntityAccess())
//        {
//            var newTransform = new LocalTransform
//            {
//                Position = float3.zero,
//                Rotation = quaternion.EulerXYZ(prepare.StartRotation),
//                Scale = 1f

//            };
//            //ecb.SetComponent(tank.Model, newTransform);
//            ecb.RemoveComponent<PrepareTank>(entity);
//        }
//    }

//}