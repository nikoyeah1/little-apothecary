using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public HerbData herb;
    public int quantity;

    public InventorySlot(HerbData herb, int quantity = 1)
    {
        this.herb     = herb;
        this.quantity = quantity;
    }
}

public class Inventory : MonoBehaviour
{
    public event Action OnInventoryChanged;

    [Header("Capacity")]
    [Tooltip("Maximum number of distinct herb types the pack can hold. " +
             "Quantity per type is unlimited as long as weight allows.")]
    public int maxSlots = 12;

    private List<InventorySlot> _slots = new List<InventorySlot>();

    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();

        if (_playerController == null)
            Debug.LogError("[Inventory] No PlayerController found on the same GameObject.");
    }

    public bool TryAddHerb(HerbData herb, int quantity = 1)
    {
        if (herb == null) return false;

        float totalWeight = herb.weight * quantity;
        if (_playerController != null && !_playerController.TryAddWeight(totalWeight))
        {
            Debug.Log($"[Inventory] Pack too heavy to add {herb.herbName}.");
            return false;
        }

        InventorySlot existing = _slots.Find(s => s.herb == herb);
        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            if (_slots.Count >= maxSlots)
            {
                _playerController?.RemoveWeight(totalWeight);
                Debug.Log($"[Inventory] No free slots for {herb.herbName}.");
                return false;
            }

            _slots.Add(new InventorySlot(herb, quantity));
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int RemoveHerb(HerbData herb, int quantity = 1)
    {
        InventorySlot slot = _slots.Find(s => s.herb == herb);
        if (slot == null) return 0;

        int removed = Mathf.Min(slot.quantity, quantity);
        slot.quantity -= removed;

        _playerController?.RemoveWeight(herb.weight * removed);

        if (slot.quantity <= 0)
            _slots.Remove(slot);

        OnInventoryChanged?.Invoke();
        return removed;
    }

    public void ClearAll()
    {
        _slots.Clear();
        OnInventoryChanged?.Invoke();
    }

    public IReadOnlyList<InventorySlot> GetSlots() => _slots.AsReadOnly();

    public int GetQuantity(HerbData herb)
    {
        InventorySlot slot = _slots.Find(s => s.herb == herb);
        return slot?.quantity ?? 0;
    }

    public bool HasHerb(HerbData herb) => GetQuantity(herb) > 0;

    public bool IsEmpty() => _slots.Count == 0;

    public int SlotCount => _slots.Count;
}
