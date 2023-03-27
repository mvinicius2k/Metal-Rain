using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct ExplorationMoveSystem : ISystem
{
    private EntityManager entityManager;

    public ExplorationMoveSystem(EntityManager entityManager)
    {
        this.entityManager = entityManager;
    }

    public void Update(ref SystemState state)
    {
         
        
    }
}

