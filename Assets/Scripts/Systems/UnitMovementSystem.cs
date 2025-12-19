using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

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
        float3 pos = localTransform.Position;
        float3 target = unitStats.targetPosition;

        // XZ movement only
        float3 toTarget = target - pos;
        toTarget.y = 0f;

        float reachedTargetDistanceSq = 2f; // tune
        if (math.lengthsq(toTarget) < reachedTargetDistanceSq)
        {
            physicsVelocity.Linear.x = 0f;
            physicsVelocity.Linear.z = 0f;

            physicsVelocity.Angular = float3.zero;
            return;
        }

        float3 moveDir = math.normalize(toTarget);

        // Rotate to face movement direction (safe because we keep it purely yaw)
        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDir, math.up()),
            deltaTime * unitStats.RotationSpeed
        );

        // Drive only horizontal velocity; preserve vertical velocity from physics
        physicsVelocity.Linear.x = moveDir.x * unitStats.MoveSpeed;
        physicsVelocity.Linear.z = moveDir.z * unitStats.MoveSpeed;
        // physicsVelocity.Linear.y is left untouched on purpose

        physicsVelocity.Angular = float3.zero;
    }
}