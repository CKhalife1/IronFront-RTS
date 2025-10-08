using Unity.Entities;
using UnityEngine;

public class ConscriptAuthoring : MonoBehaviour
{
    public class Baker : Baker<ConscriptAuthoring>
    {
        public override void Bake(ConscriptAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ConscriptTag>(entity);
        }
    }
}

public struct ConscriptTag : IComponentData { }
