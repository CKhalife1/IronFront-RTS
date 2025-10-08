using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitStatsAuthoring : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float rotationSpeed = 5f;
    public float health = 100f;
    public float attackPower = 10f;
    public int unitType = 0;

    public class Baker : Baker<UnitStatsAuthoring>
    {
        public override void Bake(UnitStatsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitStatsComponent
            {
                MoveSpeed = authoring.moveSpeed,
                RotationSpeed = authoring.rotationSpeed,
                Health = authoring.health,
                AttackPower = authoring.attackPower,
                UnitType = authoring.unitType
            });
        }
    }
}

public struct UnitStatsComponent : IComponentData
{
    public float3 targetPosition;
    public float MoveSpeed;
    public float RotationSpeed;
    public float Health;
    public float AttackPower;
    public int UnitType; // 0 = Soldier, 1 = Tank, etc.
}

