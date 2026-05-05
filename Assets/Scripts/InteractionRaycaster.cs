using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionRaycaster : MonoBehaviour
{

    [Header("Detection")]
    [Tooltip("Maximum distance at which the player can interact with objects.")]
    public float interactRange = 6.0f;

    [Tooltip("Radius of the detection sphere. 0 = thin ray (precise). " +
             "0.4-0.6 = forgiving overlap that doesn't feel like cheating.")]
    public float sphereRadius = 0.5f;

    [Tooltip("Layers the cast can hit. Make sure Player layer is UNCHECKED.")]
    public LayerMask interactMask;

    [Header("Debug")]
    [Tooltip("Draws the detection sphere in the Scene view while in Play mode.")]
    public bool drawDebugRay = true;

    private Camera        _camera;
    private IInteractable _currentTarget;
    private HerbInfoHUD   _herbInfoHUD;
    private Collider[]    _selfColliders;

    private RaycastHit[]  _hitBuffer = new RaycastHit[16];

    void Start()
    {
        _camera = Camera.main;

        if (_camera == null)
            Debug.LogError("[InteractionRaycaster] No camera tagged MainCamera found.");

        _selfColliders = GetComponentsInChildren<Collider>(includeInactive: true);

        if (interactMask.value == 0)
        {
            Debug.LogWarning("[InteractionRaycaster] Interact Mask is Nothing - " +
                             "falling back to Everything.");
            interactMask = ~0;
        }

        HerbInfoHUD[] huds = FindObjectsByType<HerbInfoHUD>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (huds.Length > 0)
            _herbInfoHUD = huds[0];
        else
            Debug.LogWarning("[InteractionRaycaster] No HerbInfoHUD found in scene.");
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (_currentTarget != null) 
                ClearCurrentTarget();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        ScanForInteractable();
        HandleInteractInput();
    }

    void ScanForInteractable()
    {
        if (_camera == null) return;

        Vector3 origin    = _camera.transform.position;
        Vector3 direction = _camera.transform.forward;

        int hitCount = Physics.SphereCastNonAlloc(
            origin, sphereRadius, direction,
            _hitBuffer, interactRange, interactMask);

        if (drawDebugRay)
        {
            Debug.DrawRay(origin, direction * interactRange,
                _currentTarget != null ? Color.green : Color.yellow);

            if (_currentTarget != null)
                Debug.DrawLine(origin + direction * interactRange,
                               origin + direction * interactRange + Vector3.up * sphereRadius,
                               Color.green);
        }

        System.Array.Sort(_hitBuffer, 0, hitCount,
            Comparer<RaycastHit>.Create((a, b) =>
                a.distance.CompareTo(b.distance)));

        IInteractable found = null;

        for (int i = 0; i < hitCount; i++)
        {
            if (IsSelfCollider(_hitBuffer[i].collider)) continue;

            IInteractable interactable =
                _hitBuffer[i].collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                found = interactable;
                break;
            }
        }

        if (found != null)
        {
            if (ReferenceEquals(found, _currentTarget)) return;

            ClearCurrentTarget();
            _currentTarget = found;
            _currentTarget.NotifyLookedAt();

            _herbInfoHUD?.ShowInfo(
                _currentTarget.GetInteractLabel(),
                _currentTarget.GetDescription());
        }
        else
        {
            if (_currentTarget != null)
                ClearCurrentTarget();
        }
    }

    void HandleInteractInput()
    {
        if (_currentTarget == null) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            Debug.Log($"[InteractionRaycaster] Interacting with: " +
                      $"{_currentTarget.GetInteractLabel()}");
            _currentTarget.Interact(gameObject);
        }
    }

    bool IsSelfCollider(Collider col)
    {
        foreach (Collider self in _selfColliders)
            if (self == col) return true;
        return false;
    }

    void ClearCurrentTarget()
    {
        _currentTarget?.NotifyLookedAway();
        _herbInfoHUD?.HideInfo();
        _currentTarget = null;
    }

    public IInteractable GetCurrentTarget() => _currentTarget;
}
