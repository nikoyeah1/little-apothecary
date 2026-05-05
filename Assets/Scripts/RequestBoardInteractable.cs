using UnityEngine;

public class RequestBoardInteractable : MonoBehaviour, IInteractable
{
    private bool         _isHighlighted;
    private MeshRenderer _meshRenderer;
    private Material     _originalMaterial;

    [Header("Highlight")]
    public Material highlightMaterial;

    [Header("Audio")]
    public AudioClip openSound;

    void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_meshRenderer != null)
            _originalMaterial = _meshRenderer.sharedMaterial;
    }

    public string GetInteractLabel() => "Request Board";
    public string GetDescription()   => "View active orders from the palace.";

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
        if (openSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXAtPoint(openSound, transform.position);

        RequestBoardHUD hud = FindFirstObjectByType<RequestBoardHUD>(
            FindObjectsInactive.Include);

        hud?.Open();
    }
}
