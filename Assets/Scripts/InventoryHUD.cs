using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventoryHUD : MonoBehaviour
{

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Grid")]
    [SerializeField] private Transform  slotGrid;
    [SerializeField] private GameObject slotTemplate;

    [Header("Slot Visuals")]
    [Tooltip("Fallback color for herb slots that have no icon assigned.")]
    public Color fallbackSlotColor = new Color(0.3f, 0.5f, 0.2f);

    private Inventory            _inventory;
    private bool                 _isOpen = false;
    private List<GameObject>     _spawnedSlots = new List<GameObject>();

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _inventory = player.GetComponent<Inventory>();
            if (_inventory != null)
                _inventory.OnInventoryChanged += RefreshIfOpen;
        }

        if (slotTemplate != null)
            slotTemplate.SetActive(false);

        inventoryPanel?.SetActive(false);
    }

    void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= RefreshIfOpen;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.tabKey.wasPressedThisFrame)
            ToggleInventory();
    }

    void ToggleInventory()
    {
        if (_isOpen) CloseInventory();
        else         OpenInventory();
    }

    void OpenInventory()
    {
        _isOpen = true;
        inventoryPanel?.SetActive(true);

        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        BuildGrid();
    }

    void CloseInventory()
    {
        _isOpen = false;
        inventoryPanel?.SetActive(false);

        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            Time.timeScale   = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void RefreshIfOpen()
    {
        if (_isOpen) BuildGrid();
    }

    void BuildGrid()
    {
        if (slotTemplate == null || slotGrid == null) return;

        foreach (GameObject slot in _spawnedSlots)
            Destroy(slot);
        _spawnedSlots.Clear();

        if (_inventory == null) return;

        IReadOnlyList<InventorySlot> slots = _inventory.GetSlots();

        foreach (InventorySlot entry in slots)
        {
            GameObject slotGO = Instantiate(slotTemplate, slotGrid);
            slotGO.SetActive(true);
            _spawnedSlots.Add(slotGO);

            Image bgImage = slotGO.transform.Find("SlotBG")?.GetComponent<Image>();
            if (bgImage != null)
                bgImage.color = entry.herb.slotColor != Color.clear
                    ? entry.herb.slotColor
                    : fallbackSlotColor;

            Image iconImage = slotGO.transform.Find("SlotIcon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                if (entry.herb.inventoryIcon != null)
                {
                    iconImage.sprite  = entry.herb.inventoryIcon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            TextMeshProUGUI nameText = slotGO.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = entry.herb.herbName;

            TextMeshProUGUI quantityText = slotGO.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            if (quantityText != null)
                quantityText.text = $"x{entry.quantity}";
        }

        if (slots.Count == 0)
        {
            GameObject emptySlot = Instantiate(slotTemplate, slotGrid);
            emptySlot.SetActive(true);
            _spawnedSlots.Add(emptySlot);

            TextMeshProUGUI nameText = emptySlot.transform.Find("NameText")
                ?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = "Pack is empty";

            TextMeshProUGUI qtyText = emptySlot.transform.Find("QuantityText")
                ?.GetComponent<TextMeshProUGUI>();
            if (qtyText != null) qtyText.text = "";
        }
    }
}
