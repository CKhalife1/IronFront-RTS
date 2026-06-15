using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                health = authoring.health,
                maxHealth = authoring.maxHealth,
                onHealthChanged = true,
            });
        }
    }
}

public struct Health : IComponentData
{
    public int health;
    public int maxHealth;
    
    public bool onHealthChanged;
}
