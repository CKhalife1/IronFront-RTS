using Unity.Entities;
using UnityEngine;

public class IronAllianceAuthoring : MonoBehaviour
{
    public class Baker : Baker<IronAllianceAuthoring>
    {
        public override void Bake(IronAllianceAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new IronAlliance());
        }
    }
}

public struct IronAlliance : IComponentData
{

}

