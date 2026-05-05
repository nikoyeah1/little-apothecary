using System;
using System.Collections.Generic;
using UnityEngine;

public class PalaceStorage : MonoBehaviour
{
    public static PalaceStorage Instance { get; private set; }

    public event Action OnStorageChanged;

    private Dictionary<HerbData, int> _stored = new Dictionary<HerbData, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void DepositAll(Inventory inventory)
    {
        if (inventory == null || inventory.IsEmpty()) return;

        foreach (InventorySlot slot in inventory.GetSlots())
            AddHerb(slot.herb, slot.quantity);

        PlayerController pc = inventory.GetComponent<PlayerController>();
        pc?.RemoveWeight(pc.currentWeight);
        inventory.ClearAll();

        OnStorageChanged?.Invoke();
    }

    public void AddHerb(HerbData herb, int quantity)
    {
        if (herb == null || quantity <= 0) return;
        if (_stored.ContainsKey(herb)) _stored[herb] += quantity;
        else _stored[herb] = quantity;
        OnStorageChanged?.Invoke();
    }

    public bool CanCraft(MedicineData medicine)
    {
        if (medicine == null) return false;
        foreach (var ing in medicine.ingredients)
            if (GetQuantity(ing.herb) < ing.quantity) return false;
        return true;
    }

    public bool ConsumeIngredients(MedicineData medicine)
    {
        if (!CanCraft(medicine)) return false;
        foreach (var ing in medicine.ingredients)
        {
            _stored[ing.herb] -= ing.quantity;
            if (_stored[ing.herb] <= 0) _stored.Remove(ing.herb);
        }
        OnStorageChanged?.Invoke();
        return true;
    }

    public int GetQuantity(HerbData herb)
    {
        if (herb == null) return 0;
        return _stored.TryGetValue(herb, out int qty) ? qty : 0;
    }

    public bool HasHerb(HerbData herb) => GetQuantity(herb) > 0;

    public IReadOnlyDictionary<HerbData, int> GetAllStored() => _stored;

    public void ClearStorage()
    {
        _stored.Clear();
        OnStorageChanged?.Invoke();
    }
}
