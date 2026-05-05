using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PunishmentHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject      punishmentPanel;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI headlineText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI continueText;

    [Header("Fade")]
    public float fadeInDuration  = 0.6f;
    public float fadeOutDuration = 0.8f;

    private CanvasGroup _canvasGroup;
    private Action      _onComplete;
    private bool        _waitingForInput = false;

    void Awake()
    {
        if (punishmentPanel != null)
        {
            _canvasGroup = punishmentPanel.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = punishmentPanel.AddComponent<CanvasGroup>();
        }

        punishmentPanel?.SetActive(false);
    }

    void Update()
    {
        if (!_waitingForInput) return;

        if (UnityEngine.InputSystem.Keyboard.current?.anyKey.wasPressedThisFrame == true)
        {
            _waitingForInput = false;
            StartCoroutine(FadeOutAndComplete());
        }
    }

    public void Show(PunishmentReason reason, int expiredRequests,
                     float minDisplayDuration, Action onComplete)
    {
        _onComplete = onComplete;
        BuildContent(reason, expiredRequests);
        StartCoroutine(ShowRoutine(minDisplayDuration));
    }

    void BuildContent(PunishmentReason reason, int expiredRequests)
    {
        DayNightCycle dayNight = FindFirstObjectByType<DayNightCycle>();
        int day = dayNight != null ? dayNight.DayNumber : 1;

        if (dayText != null)
            dayText.text = $"Day {day}";

        switch (reason)
        {
            case PunishmentReason.MissedRequests:
                if (headlineText) headlineText.text = "Orders Unfulfilled.";
                if (bodyText)
                {
                    string plural = expiredRequests == 1 ? "order" : "orders";
                    bodyText.text =
                        $"You failed to deliver {expiredRequests} {plural} before curfew.\n\n" +
                        "The palace staff made do without you. You were confined to your " +
                        "quarters until late morning as a reminder of your obligations.";
                }
                break;

            case PunishmentReason.CurfewBreach:
                if (headlineText) headlineText.text = "Returned After Midnight.";
                if (bodyText)
                    bodyText.text =
                        "The palace guards caught you returning well after midnight.\n\n" +
                        "You were escorted to your room and kept there until the sun " +
                        "was already high. The morning was lost.";
                break;
        }

        if (continueText)
            continueText.text = "Press any key to continue...";
    }

    IEnumerator ShowRoutine(float minDuration)
    {
        punishmentPanel?.SetActive(true);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = false;

        yield return FadeRoutine(0f, 1f, fadeInDuration);

        float elapsed = 0f;
        while (elapsed < minDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _waitingForInput = true;
    }

    IEnumerator FadeOutAndComplete()
    {
        yield return FadeRoutine(1f, 0f, fadeOutDuration);

        punishmentPanel?.SetActive(false);

        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        _onComplete?.Invoke();
    }

    IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (_canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed            += Time.unscaledDeltaTime;
            _canvasGroup.alpha  = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }
}
