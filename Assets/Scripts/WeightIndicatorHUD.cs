using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeightIndicatorHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject      weightPanel;
    [SerializeField] private Slider          fillBar;
    [SerializeField] private TextMeshProUGUI weightText;

    [Header("Timing")]
    [Tooltip("How long the indicator stays visible after a weight change.")]
    public float displayDuration = 3f;

    public float fadeInDuration  = 0.2f;
    public float fadeOutDuration = 0.8f;

    [Header("Color Feedback")]
    [Tooltip("Fill bar color when pack is light.")]
    public Color lightColor  = new Color(0.4f, 0.8f, 0.3f);

    [Tooltip("Fill bar color when the pack is heavy (>75% full).")]
    public Color heavyColor  = new Color(0.9f, 0.4f, 0.1f);

    private CanvasGroup     _canvasGroup;
    private PlayerController _playerController;
    private Inventory        _inventory;
    private Coroutine        _displayCoroutine;
    private float            _lastWeightRatio = 0f;

    private Image _fillImage;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerController = player.GetComponent<PlayerController>();
            _inventory        = player.GetComponent<Inventory>();

            if (_inventory != null)
                _inventory.OnInventoryChanged += HandleInventoryChanged;
        }
        else
        {
            Debug.LogWarning("[WeightIndicatorHUD] No Player tagged GameObject found.");
        }

        if (weightPanel != null)
        {
            _canvasGroup = weightPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = weightPanel.AddComponent<CanvasGroup>();
        }

        if (fillBar != null)
        {
            Transform fill = fillBar.transform.Find("Fill Area/Fill");
            if (fill != null) _fillImage = fill.GetComponent<Image>();
        }

        if (fillBar != null)
        {
            fillBar.minValue    = 0f;
            fillBar.maxValue    = 1f;
            fillBar.interactable = false;
        }

        HideInstant();
    }

    void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= HandleInventoryChanged;
    }

    void HandleInventoryChanged()
    {
        if (_playerController == null) return;

        float ratio = _playerController.GetWeightRatio();
        UpdateVisuals(ratio);

        if (_displayCoroutine != null)
            StopCoroutine(_displayCoroutine);

        _displayCoroutine = StartCoroutine(DisplayRoutine());
    }

    void UpdateVisuals(float ratio)
    {
        if (fillBar != null)
            fillBar.value = ratio;

        if (_fillImage != null)
            _fillImage.color = Color.Lerp(lightColor, heavyColor, ratio);

        if (weightText != null && _playerController != null)
        {
            float current = _playerController.currentWeight;
            float max     = _playerController.maxWeight;
            weightText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }
    }

    IEnumerator DisplayRoutine()
    {
        weightPanel?.SetActive(true);

        yield return FadeRoutine(0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(displayDuration);

        yield return FadeRoutine(1f, 0f, fadeOutDuration);

        weightPanel?.SetActive(false);
    }

    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (_canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed            += Time.deltaTime;
            _canvasGroup.alpha  = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }

    void HideInstant()
    {
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        weightPanel?.SetActive(false);
    }
}
