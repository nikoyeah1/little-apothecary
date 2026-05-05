using UnityEngine;

public class RestInteractable : MonoBehaviour, IInteractable
{
    [Header("Highlight")]
    public Material highlightMaterial;

    [Header("Audio")]
    public AudioClip restSound;

    private bool         _isHighlighted;
    private MeshRenderer _meshRenderer;
    private Material     _originalMaterial;

    void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_meshRenderer != null)
            _originalMaterial = _meshRenderer.sharedMaterial;
    }

    public string GetInteractLabel() => "Bed";

    public string GetDescription()
    {
        DayNightCycle dayNight = FindFirstObjectByType<DayNightCycle>();

        if (dayNight != null && dayNight.IsNight)
            return "Rest until morning.";

        return $"It is still daytime. Rest until tomorrow morning?";
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
        DayNightCycle dayNight = FindFirstObjectByType<DayNightCycle>();
        if (dayNight == null) return;

        AudioManager.Instance?.PlaySFXAtPoint(restSound, transform.position);

        dayNight.SkipToMorning();
        
        SaveManager.Instance?.Save(0);

        Debug.Log("[RestInteractable] Player rested until morning.");
    }
}
