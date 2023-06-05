using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//public partial struct SelectionSystem : ISystem
//{
//    //public void OnCreate(ref SystemState state)
//    //{
//    //    state.RequireForUpdate<WorldSelection>();
//    //}
       
//    //public void OnUpdate(ref SystemState state)
//    //{

//    //    var inputData = SystemAPI.GetSingleton<WorldSelection>();
//    //    if (!inputData.Active)
//    //        return;

//    //    var selectionEntity = SystemAPI.GetSingletonEntity<WorldSelection>();
//    //    var plane = SystemAPI.GetComponent<SelectionTile>(selectionEntity);
        
//    //    var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        
//    //    if (physicsWorld.CastRay(inputData.Start, out var startHit))
//    //    {
//    //        Debug.Log($"Posição de Início: {startHit.Position}");
//    //    }

//    //    if (physicsWorld.CastRay(inputData.End, out var endHit))
//    //    {
//    //        Debug.Log($"Posição de Fim: {endHit.Position}");
//    //    }

//    //    var size = math.abs(startHit.Position - endHit.Position);
//    //    state.EntityManager.SetComponentData(plane.Tile, new LocalToWorld
//    //    {
//    //        Value = float4x4.TRS(
//    //            translation: (startHit.Position / endHit.Position) / 2f,
//    //            rotation: quaternion.identity,
//    //            scale: size),
//    //    });





//    //}
//}