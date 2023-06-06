using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

//[BurstCompile]
//public partial struct TankMovementSystem : ISystem
//{
//    public void OnUpdate(ref SystemState state)
//    {
//        //var singleton = SystemAPI.GetSingleton<BeginVariableRateSimulationEntityCommandBufferSystem.Singleton>();
//        //var ecb = singleton.CreateCommandBuffer(state.WorldUnmanaged);

//        //var models = new EntityQueryBuilder(Allocator.Temp).WithAspect<TankModelAspect>().Build(ref state);

//        foreach (var item in SystemAPI.Query<TankModelAspect>())
//        {
//            item.TryAim();
//        }
//    }
//}
