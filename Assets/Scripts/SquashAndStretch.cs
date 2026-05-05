using UnityEngine;
using UnityEngine.InputSystem;

public class SquashAndStretch : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The child GameObject holding the visible mesh.")]
    public Transform modelRoot;

    [Header("Bob Settings")]
    [Tooltip("How strongly the model stretches vertically at peak.")]
    public float stretchAmount = 0.12f;

    [Tooltip("How strongly the model squashes at the bottom of the cycle.")]
    public float squashAmount = 0.08f;

    [Tooltip("Bob cycles per second at normal walk speed.")]
    public float walkFrequency = 2.4f;

    [Tooltip("Bob cycles per second while sprinting.")]
    public float sprintFrequency = 3.8f;

    [Tooltip("How quickly the effect fades in/out when starting/stopping.")]
    public float blendSpeed = 6f;

    [Header("Heavy Pack")]
    [Tooltip("Extra downward squash when pack is full - makes the player look burdened.")]
    public float heavyPackSquashBonus = 0.06f;

    [Tooltip("Weight ratio above which the heavy squash kicks in.")]
    [Range(0f, 1f)] public float heavyPackThreshold = 0.6f;

    [Header("Landing")]
    [Tooltip("Brief squash applied the frame the player lands.")]
    public float landingSquash = 0.18f;

    [Tooltip("How quickly the landing squash recovers.")]
    public float landingRecoverySpeed = 8f;

    private PlayerController _playerController;
    private Vector3          _baseScale;
    private float            _sineTime     = 0f;
    private float            _currentBlend = 0f;
    private float            _landingOffset = 0f;
    private bool             _wasGrounded  = true;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();

        if (modelRoot == null)
        {
            Debug.LogError("[SquashAndStretch] No ModelRoot assigned.");
            enabled = false;
            return;
        }

        _baseScale = modelRoot.localScale;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        bool  isMoving    = IsMoving();
        bool  isSprinting = IsSprinting();
        float weightRatio = _playerController != null
            ? _playerController.GetWeightRatio() : 0f;

        bool isGrounded = CheckGrounded();
        if (!_wasGrounded && isGrounded)
            _landingOffset = -landingSquash;
        _wasGrounded = isGrounded;

        _landingOffset = Mathf.MoveTowards(
            _landingOffset, 0f, landingRecoverySpeed * Time.deltaTime);

        _currentBlend = Mathf.MoveTowards(
            _currentBlend, isMoving ? 1f : 0f, blendSpeed * Time.deltaTime);

        float freq = isSprinting ? sprintFrequency : walkFrequency;
        if (isMoving)
            _sineTime += Time.deltaTime * freq * Mathf.PI * 2f;

        float sine = Mathf.Sin(_sineTime);

        float verticalOffset = sine >= 0f
            ?  sine * stretchAmount
            :  sine * squashAmount;

        float heavyBias = 0f;
        if (weightRatio > heavyPackThreshold)
        {
            float t = Mathf.InverseLerp(heavyPackThreshold, 1f, weightRatio);
            heavyBias = -t * heavyPackSquashBonus;
        }

        float totalVertical = (verticalOffset + heavyBias) * _currentBlend
                              + _landingOffset;

        float scaleY  = Mathf.Max(_baseScale.y * 0.5f, _baseScale.y + totalVertical);
        float scaleXZ = Mathf.Max(_baseScale.x * 0.5f, _baseScale.x - totalVertical * 0.5f);

        modelRoot.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);
    }

    bool IsMoving()
    {
        var kb = Keyboard.current;
        return kb != null && (kb.wKey.isPressed || kb.aKey.isPressed ||
                               kb.sKey.isPressed || kb.dKey.isPressed);
    }

    bool IsSprinting()
    {
        var kb = Keyboard.current;
        return IsMoving() && kb != null && kb.leftShiftKey.isPressed;
    }

    bool CheckGrounded() =>
        Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.35f);

    void OnDisable()
    {
        if (modelRoot != null)
            modelRoot.localScale = _baseScale;
    }
}
