using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

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

