using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSingleton : MonoBehaviour
{
    public static CameraSingleton Instance;
    public float MoveSensibility = 1f;
    public float RotationSensibility = 1f;
    

    private Camera mainCamara;
    

    

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        mainCamara = Camera.main;
        
        //Transferir para um baker
        //entity = world.EntityManager.GetComponentData(new WorldSelection { Tile = world.EntityManager. }, new FixedString64Bytes("World Selection Singleton"));
        
        
    }

    

    public RaycastInput GetWorldRaycastInput(Vector2 screenClickPosition)
    {
        var ray = mainCamara.ScreenPointToRay(screenClickPosition);

        var camRayEndPoint = ray.GetPoint(mainCamara.farClipPlane);
        
        
        var input = new RaycastInput
        {
            Start = ray.origin,
            Filter = CollisionFilter.Default,
            End = camRayEndPoint
        };

        return input;
    }



    
    
}

