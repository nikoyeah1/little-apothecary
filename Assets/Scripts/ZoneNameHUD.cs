using System.Collections;
using UnityEngine;
using TMPro;

public class ZoneNameHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI zoneNameText;

    [Header("Timing")]
    [Tooltip("How long the zone name stays fully visible.")]
    public float displayDuration = 2.5f;

    [Tooltip("How long the fade-in takes.")]
    public float fadeInDuration = 0.4f;

    [Tooltip("How long the fade-out takes.")]
    public float fadeOutDuration = 1.0f;

    private Coroutine _displayCoroutine;

    void Start()
    {
        if (zoneNameText == null)
        {
            Debug.LogWarning("[ZoneNameHUD] ZoneNameText not assigned.");
            return;
        }

        SetAlpha(0f);

        if (ZoneManager.Instance != null)
            ZoneManager.Instance.OnZoneChanged += HandleZoneChanged;
        else
            Debug.LogWarning("[ZoneNameHUD] ZoneManager not found.");
    }

    void OnDestroy()
    {
        if (ZoneManager.Instance != null)
            ZoneManager.Instance.OnZoneChanged -= HandleZoneChanged;
    }

    void HandleZoneChanged(ZoneType previous, ZoneType current)
    {
        if (current == ZoneType.None) return;

        string label = FormatZoneName(current);

        if (_displayCoroutine != null)
            StopCoroutine(_displayCoroutine);

        _displayCoroutine = StartCoroutine(DisplayRoutine(label));
    }

    IEnumerator DisplayRoutine(string label)
    {
        zoneNameText.text = label;

        yield return FadeRoutine(0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(displayDuration);

        yield return FadeRoutine(1f, 0f, fadeOutDuration);
    }

    IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration));
            yield return null;
        }
        SetAlpha(toAlpha);
    }

    void SetAlpha(float alpha)
    {
        if (zoneNameText == null) return;
        Color c = zoneNameText.color;
        c.a = alpha;
        zoneNameText.color = c;
    }

    string FormatZoneName(ZoneType zone)
    {
        return zone switch
        {
            ZoneType.PalaceGrounds => "Palace Grounds",
            ZoneType.Plains        => "The Plains",
            ZoneType.Forest        => "The Forest",
            ZoneType.RiverLake     => "The River",
            ZoneType.Mountain      => "The Mountain",
            _                      => zone.ToString()
        };
    }
}
