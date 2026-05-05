using UnityEngine;

public class HerbPickup : MonoBehaviour, IInteractable
{

    [Tooltip("The data asset defining this herb's name, weight, description, etc.")]
    public HerbData herbData;

    [Tooltip("How many units of this herb this instance gives on pickup.")]
    public int quantity = 1;

    [Tooltip("If true, the GameObject is destroyed after pickup. " +
             "Set false for herbs that should regrow.")]
    public bool destroyOnPickup = true;

    [Header("Visual Feedback")]
    [Tooltip("Optional: material applied to the herb mesh while the player is looking at it.")]
    public Material highlightMaterial;

    private MeshRenderer _meshRenderer;
    private Material     _originalMaterial;
    private bool         _isHighlighted = false;

    void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (_meshRenderer != null)
            _originalMaterial = _meshRenderer.sharedMaterial;
    }

    public string GetInteractLabel()
    {
        if (herbData == null) return "Unknown Herb";
        return herbData.herbName;
    }

    public string GetDescription()
    {
        if (herbData == null) return "";
        return herbData.description;
    }

    public void NotifyLookedAt()
    {
        if (_isHighlighted) return;
        _isHighlighted = true;
        ApplyHighlight(true);
    }

    public void NotifyLookedAway()
    {
        if (!_isHighlighted) return;
        _isHighlighted = false;
        ApplyHighlight(false);
    }

    public void Interact(GameObject player)
    {
        if (herbData == null)
        {
            Debug.LogWarning($"[HerbPickup] {gameObject.name} has no HerbData assigned.");
            return;
        }

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("[HerbPickup] Player has no Inventory component.");
            return;
        }

        bool success = inventory.TryAddHerb(herbData, quantity);

        if (success)
        {
            if (herbData.pickupSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFXAtPoint(herbData.pickupSound, transform.position);

            if (destroyOnPickup)
                Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[HerbPickup] Could not pick up {herbData.herbName} - pack full or too heavy.");
        }
    }

    void ApplyHighlight(bool on)
    {
        if (_meshRenderer == null) return;

        if (on && highlightMaterial != null)
            _meshRenderer.sharedMaterial = highlightMaterial;
        else
            _meshRenderer.sharedMaterial = _originalMaterial;
    }
}
