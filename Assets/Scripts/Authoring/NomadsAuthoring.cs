using Unity.Entities;
using UnityEngine;

public class NomadsAuthoring : MonoBehaviour
{
    public class Baker : Baker<NomadsAuthoring>
    {
        public override void Bake(NomadsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Nomads());
        }
    }
}

public struct Nomads : IComponentData
{

}

