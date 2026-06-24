using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(UnitMovementSystem))]
partial struct MoveOverrideSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRO<MoveOverride> moveOverride,
            EnabledRefRW<MoveOverride> moveOverrideEnabled,
            RefRW<UnitStatsComponent> unitStats)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRO<MoveOverride>,
                EnabledRefRW<MoveOverride>,
                RefRW<UnitStatsComponent>>())
        {
            float3 toTarget = moveOverride.ValueRO.targetPosition - localTransform.ValueRO.Position;
            toTarget.y = 0f;

            if (math.lengthsq(toTarget) > math.square(unitStats.ValueRO.StopDistance))
            {
                unitStats.ValueRW.targetPosition = moveOverride.ValueRO.targetPosition;
            }
            else
            {
                moveOverrideEnabled.ValueRW = false;
            }
        }
    }
}
