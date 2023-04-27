using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionWorldMono : MonoBehaviour
{
    public GameObject Plane;
    public InputAction InputAction;

    private Entity entity;
    private World world;
    private WorldSelection selectionComponent;
    private CameraSingleton cameraSingleton;

    public Entity Entity  => entity;

    private void StartSelection(InputAction.CallbackContext context)
    {

        var raycast = cameraSingleton.GetWorldRaycastInput(InputAction.ReadValue<Vector2>());

        selectionComponent = new WorldSelection
        {
            Start = raycast,
            End = raycast,
            Active = true
        };

        world.EntityManager.SetComponentData(entity, selectionComponent);
    }
    private void EndSelection(InputAction.CallbackContext context)
    {
        selectionComponent = new WorldSelection
        {
            Start = selectionComponent.Start,
            End = selectionComponent.End,
            Active = false
        };
        world.EntityManager.SetComponentData(entity, selectionComponent);
    }


    private void Awake()
    {
        world = World.DefaultGameObjectInjectionWorld;

        
        entity = world.EntityManager.CreateSingleton(selectionComponent);
        Debug.Log("Seleção added");
    }

    private void OnEnable()
    {
        InputAction.started += StartSelection;
        InputAction.canceled += EndSelection;
        InputAction.Enable();
    }

    private void OnDisable()
    {
        InputAction.started -= StartSelection;
        InputAction.Disable();
    }

    private void Update()
    {
        if (InputAction.triggered)
        {
            var raycast = cameraSingleton.GetWorldRaycastInput(InputAction.ReadValue<Vector2>());
            selectionComponent = new WorldSelection
            {
                Start = selectionComponent.Start,
                End = raycast,
                Active = selectionComponent.Active
            };
            world.EntityManager.SetComponentData(entity, selectionComponent);
        }
    }
}

public class SelectionTileMonoBaker : Baker<SelectionWorldMono>
{
    public override void Bake(SelectionWorldMono authoring)
    {
        var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
        AddComponent(entity, new SelectionTile
        {
            Tile = GetEntity(authoring.Plane, TransformUsageFlags.Dynamic)
        });
       
    }
}
