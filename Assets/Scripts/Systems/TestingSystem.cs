using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct TestingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        int unitCount = 0;

        foreach (var (localTransform, unitStats, physicsVelocity)
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<UnitStatsComponent>, RefRW<PhysicsVelocity>>().WithDisabled<Selected>())
        {
            unitCount++;
        }
    }
}
