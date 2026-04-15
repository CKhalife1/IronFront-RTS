using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public int health;
    public class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                health = authoring.health,
            });
        }
    }
}

public struct Health : IComponentData
{
    public int health;
}
