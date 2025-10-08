using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEditor;

public partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMovementJob unitMovementJob = new UnitMovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };

        unitMovementJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitMovementJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform localTransform, in UnitStatsComponent unitStats, ref PhysicsVelocity physicsVelocity)
    {
        float3 targetPosition = /*MouseWorldPosition.Instance.GetPosition();*/ unitStats.targetPosition;
        float3 moveDirection = targetPosition - localTransform.Position;

        float reachedTargetDistanceSq = 2f;
        if (math.lengthsq(moveDirection) < reachedTargetDistanceSq)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation = math.slerp(localTransform.Rotation, quaternion.LookRotation(moveDirection, math.up()), deltaTime * unitStats.RotationSpeed);

        physicsVelocity.Linear = moveDirection * unitStats.MoveSpeed;
        physicsVelocity.Angular = float3.zero;
    }

}