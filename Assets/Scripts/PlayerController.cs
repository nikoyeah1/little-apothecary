using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    [Tooltip("Base walking speed in units/second at zero weight.")]
    public float baseSpeed = 5f;

    [Tooltip("Sprint speed multiplier. Sprint is disabled above the sprintWeightThreshold.")]
    public float sprintMultiplier = 1.6f;

    [Tooltip("Weight ratio (0-1) above which the player cannot sprint.")]
    [Range(0f, 1f)] public float sprintWeightThreshold = 0.5f;

    [Header("Weight System")]
    [Tooltip("Maximum carrying capacity in arbitrary weight units.")]
    public float maxWeight = 100f;

    [Tooltip("Current weight of the player's pack. Modified at runtime by herb collection.")]
    public float currentWeight = 0f;

    [Tooltip("Speed multiplier applied at maximum weight. 0.4 means 40% of base speed.")]
    [Range(0.1f, 1f)] public float maxWeightSpeedPenalty = 0.4f;

    [Header("Slope Handling")]
    [Tooltip("Maximum slope angle (degrees) the player can climb at zero weight.")]
    public float maxSlopeAngle = 45f;

    [Tooltip("How many degrees are subtracted from max slope when the pack is full.")]
    public float maxWeightSlopeReduction = 20f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("Ground Check")]
    [Tooltip("Assign the GroundCheck child transform here, positioned just below the feet.")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.35f;
    public LayerMask groundMask;

    private CharacterController _controller;
    private Transform _cameraTransform;
    private Vector3 _verticalVelocity;
    private bool _isGrounded;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;
        else
            Debug.LogWarning("[PlayerController] No MainCamera found. Check the camera's tag.");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        CheckGrounded();
        HandleMovement();
        ApplyGravity();
        UpdateSlopeLimit();
    }

    void CheckGrounded()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (_isGrounded && _verticalVelocity.y < 0f)
            _verticalVelocity.y = -2f;
    }

    void HandleMovement()
    {
        if (_cameraTransform == null) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float h = 0f;
        float v = 0f;

        if (keyboard.dKey.isPressed) h += 1f;
        if (keyboard.aKey.isPressed) h -= 1f;
        if (keyboard.wKey.isPressed) v += 1f;
        if (keyboard.sKey.isPressed) v -= 1f;

        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight   = _cameraTransform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;

        if (moveDir.magnitude >= 0.1f)
        {
            moveDir.Normalize();

            float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetYaw, 0f);

            float speed = CalculateCurrentSpeed();
            _controller.Move(moveDir * speed * Time.deltaTime);
        }
    }

    void ApplyGravity()
    {
        _verticalVelocity.y += gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);
    }

    void UpdateSlopeLimit()
    {
        _controller.slopeLimit = GetCurrentMaxSlopeAngle();
    }

    float CalculateCurrentSpeed()
    {
        float weightRatio = currentWeight / maxWeight;
        float speedMult   = Mathf.Lerp(1f, maxWeightSpeedPenalty, weightRatio);
        float speed       = baseSpeed * speedMult;

        var keyboard = Keyboard.current;
        bool canSprint = keyboard != null
                         && keyboard.leftShiftKey.isPressed
                         && weightRatio <= sprintWeightThreshold;

        if (canSprint) speed *= sprintMultiplier;

        return speed;
    }

    public float GetCurrentMaxSlopeAngle()
    {
        float weightRatio = currentWeight / maxWeight;
        float reduction   = weightRatio * maxWeightSlopeReduction;
        return Mathf.Max(15f, maxSlopeAngle - reduction);
    }

    public bool TryAddWeight(float amount)
    {
        if (currentWeight + amount > maxWeight) return false;
        currentWeight += amount;
        return true;
    }

    public void RemoveWeight(float amount)
    {
        currentWeight = Mathf.Max(0f, currentWeight - amount);
    }

    public float GetWeightRatio() => currentWeight / maxWeight;

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
