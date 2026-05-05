using UnityEngine;

public class StorageDepositInteractable : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    public AudioClip depositSound;

    private bool _isHighlighted;
    private MeshRenderer _meshRenderer;
    private Material _originalMaterial;

    [Header("Highlight")]
    public Material highlightMaterial;

    void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_meshRenderer != null)
            _originalMaterial = _meshRenderer.sharedMaterial;
    }

    public string GetInteractLabel() => "Storage Chest";

    public string GetDescription()
    {
        if (PalaceStorage.Instance == null) return "Deposit your herbs here.";

        int totalStored = 0;
        foreach (var pair in PalaceStorage.Instance.GetAllStored())
            totalStored += pair.Value;

        return totalStored > 0
            ? $"Deposit pack contents.\n({totalStored} herbs already stored)"
            : "Deposit your pack contents here.";
    }

    public void NotifyLookedAt()
    {
        if (_isHighlighted) return;
        _isHighlighted = true;
        if (_meshRenderer != null && highlightMaterial != null)
            _meshRenderer.sharedMaterial = highlightMaterial;
    }

    public void NotifyLookedAway()
    {
        if (!_isHighlighted) return;
        _isHighlighted = false;
        if (_meshRenderer != null)
            _meshRenderer.sharedMaterial = _originalMaterial;
    }

    public void Interact(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();

        if (inventory == null)
        {
            Debug.LogWarning("[StorageDeposit] No Inventory on player.");
            return;
        }

        if (inventory.IsEmpty())
        {
            Debug.Log("[StorageDeposit] Pack is already empty.");
            return;
        }

        PalaceStorage.Instance?.DepositAll(inventory);

        if (depositSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXAtPoint(depositSound, transform.position);
    }
}
