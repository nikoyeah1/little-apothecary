using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SaveLoadHUD : MonoBehaviour
{

    [Header("Panels")]
    [SerializeField] private GameObject      saveLoadPanel;
    [SerializeField] private TextMeshProUGUI modeTitle;

    [Header("Slots")]
    [SerializeField] private Transform       slotsParent;
    [SerializeField] private GameObject      slotTemplate;

    private bool         _isOpen    = false;
    private bool         _isSaveMode = true;
    private int          _openedOnFrame = -1;
    private GameObject[] _spawnedSlots = new GameObject[SaveManager.SlotCount];

    void Start()
    {
        if (slotTemplate != null) slotTemplate.SetActive(false);
        saveLoadPanel?.SetActive(false);
    }

    void Update()
    {
        if (!_isOpen) return;
        if (Time.frameCount == _openedOnFrame) return;

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Close();
    }

    public void OpenSave()  => Open(saveMode: true);
    public void OpenLoad()  => Open(saveMode: false);

    void Open(bool saveMode)
    {
        _isOpen        = true;
        _isSaveMode    = saveMode;
        _openedOnFrame = Time.frameCount;

        if (modeTitle != null)
            modeTitle.text = saveMode ? "Save Game" : "Load Game";

        saveLoadPanel?.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        BuildSlots();
    }

    public void Close()
    {
        _isOpen = false;
        saveLoadPanel?.SetActive(false);

        foreach (GameObject s in _spawnedSlots)
            if (s != null) Destroy(s);

        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void BuildSlots()
    {
        if (SaveManager.Instance == null || slotTemplate == null) return;

        for (int i = 0; i < SaveManager.SlotCount; i++)
        {
            if (_spawnedSlots[i] != null) Destroy(_spawnedSlots[i]);

            GameObject slot = Instantiate(slotTemplate, slotsParent);
            slot.SetActive(true);
            _spawnedSlots[i] = slot;

            int capturedSlot = i;
            SaveData peek    = SaveManager.Instance.PeekSlot(i);
            bool     isEmpty = peek == null;

            TextMeshProUGUI numText = slot.transform.Find("SlotNumberText")
                ?.GetComponent<TextMeshProUGUI>();
            if (numText != null) numText.text = $"Slot {i + 1}";

            TextMeshProUGUI infoText = slot.transform.Find("SlotInfoText")
                ?.GetComponent<TextMeshProUGUI>();
            if (infoText != null)
            {
                infoText.text = isEmpty
                    ? "<color=#666666>Empty</color>"
                    : $"Day {peek.dayNumber}  —  {peek.saveDateTime}";
            }

            Button actionBtn = slot.transform.Find("ActionButton")?.GetComponent<Button>();
            if (actionBtn != null)
            {
                actionBtn.interactable = _isSaveMode || !isEmpty;

                TextMeshProUGUI btnText = actionBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = _isSaveMode ? "Save" : "Load";

                actionBtn.onClick.RemoveAllListeners();
                if (_isSaveMode)
                    actionBtn.onClick.AddListener(() => OnSaveSlot(capturedSlot));
                else
                    actionBtn.onClick.AddListener(() => OnLoadSlot(capturedSlot));
            }

            Button deleteBtn = slot.transform.Find("DeleteButton")?.GetComponent<Button>();
            if (deleteBtn != null)
            {
                deleteBtn.interactable = !isEmpty;
                deleteBtn.onClick.RemoveAllListeners();
                deleteBtn.onClick.AddListener(() => OnDeleteSlot(capturedSlot));
            }
        }
    }

    void OnSaveSlot(int slot)
    {
        if (SaveManager.Instance == null) return;

        bool ok = SaveManager.Instance.Save(slot);
        if (ok)
        {
            AudioManager.Instance?.PlaySFX(null);
            BuildSlots();
        }
    }

    void OnLoadSlot(int slot)
    {
        if (SaveManager.Instance == null) return;

        bool ok = SaveManager.Instance.Load(slot);
        if (ok)
        {
            Close();
            GameManager.Instance?.ResumeGame();
        }
    }

    void OnDeleteSlot(int slot)
    {
        SaveManager.Instance?.DeleteSave(slot);
        BuildSlots();
    }
}
