using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CraftingHUD : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject craftingPanel;

    [Header("Recipe List")]
    [Tooltip("Assign the Content object inside the RecipeListParent Scroll View.")]
    [SerializeField] private Transform  recipeListContent;
    [SerializeField] private GameObject recipeTemplate;

    [Header("Storage Summary")]
    [SerializeField] private TextMeshProUGUI storageListText;

    [Header("Known Medicines")]
    [Tooltip("All medicine recipes the player can craft.")]
    public MedicineData[] knownMedicines;

    [Header("Audio")]
    public AudioClip craftSuccessSound;
    public AudioClip craftFailSound;

    private bool             _isOpen       = false;
    private int              _openedOnFrame = -1;
    private List<GameObject> _spawnedEntries = new List<GameObject>();

    void Start()
    {
        if (recipeTemplate != null)
            recipeTemplate.SetActive(false);

        craftingPanel?.SetActive(false);

        if (PalaceStorage.Instance != null)
            PalaceStorage.Instance.OnStorageChanged += RefreshIfOpen;
    }

    void OnDestroy()
    {
        if (PalaceStorage.Instance != null)
            PalaceStorage.Instance.OnStorageChanged -= RefreshIfOpen;
    }

    void Update()
    {
        if (!_isOpen) return;
        if (Time.frameCount == _openedOnFrame) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
            Close();
    }

    public void Open()
    {
        _isOpen        = true;
        _openedOnFrame = Time.frameCount;

        craftingPanel?.SetActive(true);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        BuildRecipeList();
        UpdateStorageSummary();
    }

    public void Close()
    {
        _isOpen = false;
        craftingPanel?.SetActive(false);

        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            Time.timeScale   = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void RefreshIfOpen()
    {
        if (_isOpen)
        {
            BuildRecipeList();
            UpdateStorageSummary();
        }
    }

    void BuildRecipeList()
    {
        foreach (GameObject go in _spawnedEntries) Destroy(go);
        _spawnedEntries.Clear();

        if (knownMedicines == null || recipeListContent == null) return;

        foreach (MedicineData medicine in knownMedicines)
        {
            if (medicine == null) continue;

            GameObject entry = Instantiate(recipeTemplate, recipeListContent);
            entry.SetActive(true);
            _spawnedEntries.Add(entry);

            bool craftable = PalaceStorage.Instance?.CanCraft(medicine) ?? false;

            // Background
            Image bg = entry.transform.Find("RecipeBG")?.GetComponent<Image>();
            if (bg != null)
                bg.color = craftable
                    ? new Color(0.2f, 0.35f, 0.2f, 0.9f)
                    : new Color(0.25f, 0.2f, 0.2f, 0.9f);

            // Name
            var nameText = entry.transform.Find("RecipeNameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text  = medicine.medicineName;
                nameText.color = craftable ? new Color(0.8f, 1f, 0.7f) : new Color(0.6f, 0.5f, 0.5f);
            }

            // Ingredients
            var ingText = entry.transform.Find("RecipeIngText")?.GetComponent<TextMeshProUGUI>();
            if (ingText != null)
                ingText.text = BuildIngredientLine(medicine);

            // Craft button
            Button craftBtn = entry.transform.Find("CraftButton")?.GetComponent<Button>();
            if (craftBtn != null)
            {
                craftBtn.interactable = craftable;

                MedicineData captured = medicine;
                craftBtn.onClick.RemoveAllListeners();
                craftBtn.onClick.AddListener(() => TryCraft(captured));

                var btnText = craftBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = craftable ? "Craft" : "Missing Herbs";
            }
        }
    }

    string BuildIngredientLine(MedicineData medicine)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var ing in medicine.ingredients)
        {
            int stored = PalaceStorage.Instance?.GetQuantity(ing.herb) ?? 0;
            bool ok    = stored >= ing.quantity;
            string col = ok ? "#7FC87F" : "#C87F7F";
            sb.Append($"<color={col}>{ing.herb.herbName} ×{ing.quantity}</color>  ");
        }
        return sb.ToString().TrimEnd();
    }

    void TryCraft(MedicineData medicine)
    {
        if (PalaceStorage.Instance == null || RequestManager.Instance == null) return;

        bool consumed = PalaceStorage.Instance.ConsumeIngredients(medicine);
        if (!consumed)
        {
            AudioManager.Instance?.PlaySFX(craftFailSound);
            return;
        }

        RequestManager.Instance.FulfillMedicine(medicine);
        AudioManager.Instance?.PlaySFX(craftSuccessSound);

        BuildRecipeList();
        UpdateStorageSummary();
    }

    void UpdateStorageSummary()
    {
        if (storageListText == null || PalaceStorage.Instance == null) return;

        var stored = PalaceStorage.Instance.GetAllStored();

        if (stored.Count == 0)
        {
            storageListText.text = "<color=#888888>Storage is empty.</color>";
            return;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var pair in stored)
            sb.AppendLine($"  {pair.Key.herbName}  ×{pair.Value}");

        storageListText.text = sb.ToString().TrimEnd();
    }
}
