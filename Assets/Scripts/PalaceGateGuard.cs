using UnityEngine;

public class PalaceGateGuard : MonoBehaviour, IInteractable
{

    [Header("Bribe Settings")]
    [Tooltip("Number of any single herb type required to bribe the guard.")]
    public int herbsRequiredForBribe = 3;

    [Tooltip("Collider that physically blocks the gate archway. " +
             "Enabled after curfew, disabled during the day.")]
    public Collider gateBlocker;

    [Header("Audio")]
    public AudioClip bribeSuccessSound;
    public AudioClip bribeFailSound;
    public AudioClip blockedSound;

    [Header("Highlight")]
    public Material highlightMaterial;

    private DayNightCycle  _dayNight;
    private bool           _isHighlighted;
    private MeshRenderer   _meshRenderer;
    private Material       _originalMaterial;
    private bool           _bribedThisNight = false;

    void Start()
    {
        _dayNight = FindFirstObjectByType<DayNightCycle>();

        if (_dayNight != null)
        {
            _dayNight.OnSunrise += HandleSunrise;
            _dayNight.OnCurfew  += HandleCurfew;
        }

        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_meshRenderer != null)
            _originalMaterial = _meshRenderer.sharedMaterial;

        if (gateBlocker != null)
            gateBlocker.enabled = false;
    }

    void OnDestroy()
    {
        if (_dayNight == null) return;
        _dayNight.OnSunrise -= HandleSunrise;
        _dayNight.OnCurfew  -= HandleCurfew;
    }

    void HandleCurfew()
    {
        if (gateBlocker != null)
            gateBlocker.enabled = true;

        _bribedThisNight = false;

        Debug.Log("[PalaceGateGuard] Gate closed for curfew.");
    }

    void HandleSunrise()
    {
        if (gateBlocker != null)
            gateBlocker.enabled = false;

        _bribedThisNight = false;

        Debug.Log("[PalaceGateGuard] Gate opened at sunrise.");
    }

    public string GetInteractLabel()
    {
        if (_dayNight == null || !_dayNight.IsCurfewActive)
            return "Palace Guard";

        return _bribedThisNight ? "Palace Guard" : "Palace Guard (Bribe to pass)";
    }

    public string GetDescription()
    {
        if (_dayNight == null || !_dayNight.IsCurfewActive)
            return "The guard stands watch at the palace gate.";

        if (_bribedThisNight)
            return "The guard nods. You may pass until dawn.";

        return $"It is past curfew. Offer {herbsRequiredForBribe} herbs to pass.\n" +
               $"Returning after midnight will result in punishment.";
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
        if (_dayNight == null || !_dayNight.IsCurfewActive)
        {
            Debug.Log("[PalaceGateGuard] Guard nods. Gate is open.");
            return;
        }

        if (_bribedThisNight)
        {
            Debug.Log("[PalaceGateGuard] Already bribed. Gate is open.");
            OpenGateTemporarily();
            return;
        }

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null) return;

        HerbData bribeHerb = FindBribeHerb(inventory);

        if (bribeHerb != null)
        {
            inventory.RemoveHerb(bribeHerb, herbsRequiredForBribe);
            _bribedThisNight = true;

            AudioManager.Instance?.PlaySFXAtPoint(bribeSuccessSound, transform.position);
            OpenGateTemporarily();

            Debug.Log($"[PalaceGateGuard] Bribe accepted ({herbsRequiredForBribe}× {bribeHerb.herbName}).");
        }
        else
        {
            AudioManager.Instance?.PlaySFXAtPoint(bribeFailSound, transform.position);
            Debug.Log("[PalaceGateGuard] Bribe rejected - not enough herbs.");
        }
    }

    HerbData FindBribeHerb(Inventory inventory)
    {
        foreach (InventorySlot slot in inventory.GetSlots())
        {
            if (slot.quantity >= herbsRequiredForBribe)
                return slot.herb;
        }
        return null;
    }

    void OpenGateTemporarily()
    {
        if (gateBlocker != null)
            gateBlocker.enabled = false;

        Invoke(nameof(CloseGate), 4f);
    }

    void CloseGate()
    {
        if (_dayNight != null && _dayNight.IsCurfewActive && !_bribedThisNight)
        {
            if (gateBlocker != null)
                gateBlocker.enabled = true;
        }
    }
}
