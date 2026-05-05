using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    [Header("Target")]
    [Tooltip("The transform the camera orbits. Assign the Player root.")]
    public Transform target;

    [Tooltip("World-space offset applied to the target position before orbiting.")]
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Orbit")]
    [Tooltip("Mouse sensitivity for orbiting.")]
    public float mouseSensitivity = 0.2f;

    [Tooltip("Minimum vertical angle (degrees).")]
    public float minPitch = -15f;

    [Tooltip("Maximum vertical angle (degrees).")]
    public float maxPitch = 65f;

    [Header("Zoom")]
    public float defaultZoom = 5f;
    public float minZoom = 1.5f;
    public float maxZoom = 12f;

    [Tooltip("How fast scroll-wheel changes distance.")]
    public float zoomSpeed = 0.3f;

    [Tooltip("How quickly zoom lerps to the target distance.")]
    public float zoomSmoothSpeed = 8f;

    [Header("Follow Smoothing")]
    [Tooltip("Lower = smoother/laggier pivot follow.")]
    public float followSmoothTime = 0.08f;

    [Header("Collision")]
    [Tooltip("Layers that block the camera (Terrain + Environment).")]
    public LayerMask collisionMask;

    [Tooltip("Radius of the sphere-cast used for collision.")]
    public float collisionRadius = 0.25f;

    [Tooltip("Small clearance kept between the camera and a surface.")]
    public float collisionPadding = 0.1f;

    private float _yaw;
    private float _pitch;

    private float _targetZoom;
    private float _currentZoom;

    private Vector3 _pivotVelocity;
    private Vector3 _smoothedPivotPos;

    void Start()
    {
        _targetZoom  = defaultZoom;
        _currentZoom = defaultZoom;

        _yaw   = transform.eulerAngles.y;
        _pitch = 20f;

        if (target != null)
            _smoothedPivotPos = target.position + pivotOffset;

        if (target == null)
            Debug.LogWarning("[CameraController] No target assigned.");
    }

    void LateUpdate()
    {
        if (target == null) return;

        // don't rotate the camera while the game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        ReadInput();
        SmoothFollowTarget();
        UpdateZoom();
        PositionCamera();
    }

    void ReadInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        _yaw   += mouseDelta.x * mouseSensitivity;
        _pitch -= mouseDelta.y * mouseSensitivity;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

        float scroll = mouse.scroll.ReadValue().y;
        _targetZoom  = Mathf.Clamp(_targetZoom - scroll * zoomSpeed, minZoom, maxZoom);
    }

    void SmoothFollowTarget()
    {
        Vector3 desiredPivot = target.position + pivotOffset;
        _smoothedPivotPos = Vector3.SmoothDamp(
            _smoothedPivotPos, desiredPivot,
            ref _pivotVelocity, followSmoothTime);
    }

    void UpdateZoom()
    {
        _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * zoomSmoothSpeed);
    }

    void PositionCamera()
    {
        Quaternion rotation      = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    desiredOffset = rotation * new Vector3(0f, 0f, -_currentZoom);
        Vector3    desiredPos    = _smoothedPivotPos + desiredOffset;

        Vector3 finalPosition = ResolveCollision(_smoothedPivotPos, desiredPos);

        transform.position = finalPosition;
        transform.LookAt(_smoothedPivotPos);
    }

    Vector3 ResolveCollision(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        float   distance  = direction.magnitude;
        direction.Normalize();

        if (Physics.SphereCast(from, collisionRadius, direction, out RaycastHit hit, distance, collisionMask))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - collisionPadding);
            return from + direction * safeDistance;
        }

        return to;
    }

    public void SnapYaw(float yaw) => _yaw = yaw;
    public float GetYaw() => _yaw;
}
