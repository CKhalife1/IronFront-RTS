using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    //public LayerMask unitsLayer;

    private Vector2 selectionStartMousePosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        bool leftMousePressed = Mouse.current.leftButton.wasPressedThisFrame;
        bool leftMouseReleased = Mouse.current.leftButton.wasReleasedThisFrame;
        bool rightMousePressed = Mouse.current.rightButton.wasPressedThisFrame;

        if (leftMousePressed)
        {
            selectionStartMousePosition = Mouse.current.position.ReadValue();
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }
        if (leftMouseReleased)
        {
            Vector2 selectionEndMousePosition = Mouse.current.position.ReadValue();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                //Remove Selected component from all previously selected units
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                entityManager.SetComponentData(entityArray[i], selected);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width * selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;


            if (isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransforms = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

                for (int i = 0; i < localTransforms.Length; i++)
                {
                    LocalTransform unitLocalTrasform = localTransforms[i];
                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTrasform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        //Unit is within selection area, ensure it has Selected component
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }
                }
            }
            else
            {
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UnitsLayer/*unitsLayer*/,
                        GroupIndex = 0
                    }
                };
                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity, selected);
                    }
                }
            }

            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (rightMousePressed)
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitStatsComponent, Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitStatsComponent> unitStatsArray = entityQuery.ToComponentDataArray<UnitStatsComponent>(Allocator.Temp);
            NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);
            for (int i = 0; i < unitStatsArray.Length; i++)
            {
                UnitStatsComponent unitStats = unitStatsArray[i];
                unitStats.targetPosition = movePositionArray[i];
                unitStatsArray[i] = unitStats;
            }

            entityQuery.CopyFromComponentDataArray(unitStatsArray);
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Mouse.current.position.ReadValue();

        Vector2 lowerLeftCorner = new Vector2
        (
            Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y)
        );

        Vector2 upperRightCorner = new Vector2
        (
            Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y)
        );

        return new Rect
        (
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
        );
    }

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
        if (positionCount == 0)
        {
            return positionArray;
        }

        positionArray[0] = targetPosition;
        if (positionArray.Length == 1)
        {
            return positionArray;
        }

        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while (positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;
            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * math.PI * 2f / ringPositionCount;
                float3 ringVector = math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ring + 1), 0f, 0));
                float3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                {
                    break;
                }
            }
            ring++;
        }
        return positionArray;
    }
}
