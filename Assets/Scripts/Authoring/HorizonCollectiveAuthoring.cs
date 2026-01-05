using Unity.Entities;
using UnityEngine;

public class HorizonCollectiveAuthoring : MonoBehaviour
{
    public class Baker : Baker<HorizonCollectiveAuthoring>
    {
        public override void Bake(HorizonCollectiveAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HorizonCollective());
        }
    }
}

public struct HorizonCollective : IComponentData
{

}

