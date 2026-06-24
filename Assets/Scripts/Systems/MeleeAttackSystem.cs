using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Target> target,
            RefRW<UnitStatsComponent> unitStats)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Target>,
                RefRW<UnitStatsComponent>>().WithDisabled<MoveOverride>())
        {
            if (target.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            if (!SystemAPI.Exists(target.ValueRO.targetEntity) ||
                !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.targetEntity) ||
                !SystemAPI.HasComponent<Health>(target.ValueRO.targetEntity))
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float meleeAttackDistance = 2f;
            float meleeAttackDistanceSq = math.square(meleeAttackDistance);
            bool isCloseEnoughToAttack =
                math.distancesq(localTransform.ValueRO.Position, targetLocalTransform.Position)
                <= meleeAttackDistanceSq;

            bool isTouchingTarget = false;
            if (!isCloseEnoughToAttack)
            {
                float3 dirToTarget = math.normalizesafe(
                    targetLocalTransform.Position - localTransform.ValueRO.Position);
                float distanceExtraToTestRaycast = .4f;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = localTransform.ValueRO.Position,
                    End = localTransform.ValueRO.Position + dirToTarget * (meleeAttack.ValueRO.colliderSize + distanceExtraToTestRaycast),
                    Filter = CollisionFilter.Default
                };
                raycastHitList.Clear();
                if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
                {
                    foreach (RaycastHit raycastHit in raycastHitList)
                    {
                        if (raycastHit.Entity == target.ValueRO.targetEntity)
                        {
                            //Raycast hit target, close enough to attack this entity
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }
            }

            if (!isCloseEnoughToAttack && !isTouchingTarget)
            {
                //Too far, move closer
                unitStats.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                //Close enough, attack
                unitStats.ValueRW.targetPosition = localTransform.ValueRO.Position;

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.timer > 0f)
                {
                    continue;
                }
                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.targetEntity);
                targetHealth.ValueRW.health -= meleeAttack.ValueRO.damageAmount;
                targetHealth.ValueRW.onHealthChanged = true;
            }
        }
    }
}
