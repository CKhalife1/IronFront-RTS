using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthDeadTestSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        EntityCommandBuffer entityCommandBuffer = 
        SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRO<Health> health,
            Entity entity)
            in SystemAPI.Query<
            RefRO<Health>>().WithEntityAccess())
        {
            if (health.ValueRO.health <= 0)
            {
                //The entity is dead
                entityCommandBuffer.DestroyEntity(entity);
            }
        }

        // entityCommandBuffer.Playback(state.EntityManager);
    }
}
