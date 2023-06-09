using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ParentMono : MonoBehaviour
{

}

public class ParentMonoBaker : Baker<ParentMono>
{
    public override void Bake(ParentMono authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        var parent = authoring.transform.parent.gameObject;
        AddComponent(entity, new Parent
        {
            Value = GetEntity(parent, TransformUsageFlags.Dynamic)
        });

    }
}

