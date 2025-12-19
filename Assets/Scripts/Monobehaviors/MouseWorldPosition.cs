using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorldPosition : MonoBehaviour
{
    private Camera _mainCamera;
    private Vector3 _lastValidPosition;

    public static MouseWorldPosition Instance { get; private set; }
    public LayerMask layerMask;
    private void Awake()
    {
        Instance = this;

        _mainCamera = Camera.main;
    }

    public Vector3 GetPosition()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            _lastValidPosition = hit.point;
            return hit.point;
        }

        return _lastValidPosition;
    }
}
