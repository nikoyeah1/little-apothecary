using System.Collections;
using UnityEngine;
using TMPro;

public class HerbInfoHUD : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private GameObject      infoPopup;
    [SerializeField] private TextMeshProUGUI herbNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Animation")]
    public float fadeInDuration  = 0.15f;
    public float fadeOutDuration = 0.25f;

    private CanvasGroup  _canvasGroup;
    private Coroutine    _fadeCoroutine;

    void Awake()
    {
        if (infoPopup != null)
        {
            _canvasGroup = infoPopup.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = infoPopup.AddComponent<CanvasGroup>();
        }

        HideInfo(instant: true);
    }

    public void ShowInfo(string herbName, string description)
    {
        if (herbNameText)    herbNameText.text    = herbName;
        if (descriptionText) descriptionText.text = description;
        if (promptText)      promptText.text      = "[ E ]  Pick Up";

        infoPopup?.SetActive(true);
        Fade(1f, fadeInDuration);
    }

    public void HideInfo(bool instant = false)
    {
        if (instant)
        {
            if (_canvasGroup) _canvasGroup.alpha = 0f;
            infoPopup?.SetActive(false);
            return;
        }

        Fade(0f, fadeOutDuration, deactivateOnComplete: true);
    }

    void Fade(float targetAlpha, float duration, bool deactivateOnComplete = false)
    {
        if (_canvasGroup == null) return;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration, deactivateOnComplete));
    }

    IEnumerator FadeRoutine(float targetAlpha, float duration, bool deactivateOnComplete)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed    = 0f;

        while (elapsed < duration)
        {
            elapsed            += Time.deltaTime;
            _canvasGroup.alpha  = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;

        if (deactivateOnComplete && targetAlpha <= 0f)
            infoPopup?.SetActive(false);
    }
}
