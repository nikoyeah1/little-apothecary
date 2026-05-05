using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RequestBoardHUD : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject requestBoardPanel;

    [Header("List")]
    [SerializeField] private Transform  requestListParent;
    [SerializeField] private GameObject requestEntryTemplate;

    [Header("Empty State")]
    [SerializeField] private TextMeshProUGUI emptyLabel;

    private List<GameObject> _spawnedEntries = new List<GameObject>();
    private bool             _isOpen         = false;

    private int _openedOnFrame = -1;

    void Start()
    {
        if (requestEntryTemplate != null)
            requestEntryTemplate.SetActive(false);

        requestBoardPanel?.SetActive(false);

        if (RequestManager.Instance != null)
            RequestManager.Instance.OnRequestsChanged += RefreshIfOpen;

        if (PalaceStorage.Instance != null)
            PalaceStorage.Instance.OnStorageChanged += RefreshIfOpen;
    }

    void OnDestroy()
    {
        if (RequestManager.Instance != null)
            RequestManager.Instance.OnRequestsChanged -= RefreshIfOpen;

        if (PalaceStorage.Instance != null)
            PalaceStorage.Instance.OnStorageChanged -= RefreshIfOpen;
    }

    void Update()
    {
        if (!_isOpen) return;

        if (Time.frameCount == _openedOnFrame) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            Close();
    }

    public void Open()
    {
        _isOpen        = true;
        _openedOnFrame = Time.frameCount;

        requestBoardPanel?.SetActive(true);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        BuildList();
    }

    public void Close()
    {
        _isOpen = false;
        requestBoardPanel?.SetActive(false);

        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            Time.timeScale   = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void RefreshIfOpen()
    {
        if (_isOpen) BuildList();
    }

    void BuildList()
    {
        foreach (GameObject entry in _spawnedEntries) Destroy(entry);
        _spawnedEntries.Clear();

        if (RequestManager.Instance == null) return;

        var active = RequestManager.Instance.GetActiveRequests();

        if (emptyLabel != null)
            emptyLabel.gameObject.SetActive(active.Count == 0);

        foreach (ActiveRequest req in active)
            SpawnEntry(req, completed: false);

        foreach (ActiveRequest req in RequestManager.Instance.GetCompletedRequests())
            SpawnEntry(req, completed: true);
    }

    void SpawnEntry(ActiveRequest req, bool completed)
    {
        if (requestEntryTemplate == null || requestListParent == null) return;

        GameObject entry = Instantiate(requestEntryTemplate, requestListParent);
        entry.SetActive(true);
        _spawnedEntries.Add(entry);

        var titleText = entry.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text  = completed ? $"V  {req.data.requestTitle}" : req.data.requestTitle;
            titleText.color = completed ? new Color(0.5f, 0.7f, 0.5f) : Color.white;
        }

        var flavourText = entry.transform.Find("FlavourText")?.GetComponent<TextMeshProUGUI>();
        if (flavourText != null)
        {
            flavourText.text  = completed ? "Fulfilled." : req.data.flavourText;
            flavourText.color = completed ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
        }

        var ingText = entry.transform.Find("IngredientsText")?.GetComponent<TextMeshProUGUI>();
        if (ingText != null)
            ingText.text = completed ? "" : BuildIngredientsString(req.data.requiredMedicine);

        var progressText = entry.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
        if (progressText != null)
            progressText.text = $"Delivered: {req.quantityFulfilled} / {req.data.quantity}";
    }

    string BuildIngredientsString(MedicineData medicine)
    {
        if (medicine == null) return "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<b>{medicine.medicineName}</b>");

        foreach (var ingredient in medicine.ingredients)
        {
            int  stored = PalaceStorage.Instance?.GetQuantity(ingredient.herb) ?? 0;
            bool ready  = stored >= ingredient.quantity;
            string col  = ready ? "#7FC87F" : "#C87F7F";
            string sym  = ready ? "V" : $"{stored}/{ingredient.quantity}";
            sb.AppendLine($"<color={col}>  {ingredient.herb.herbName} ×{ingredient.quantity}  {sym}</color>");
        }

        return sb.ToString().TrimEnd();
    }
}
