using System;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public enum DayPhase { Morning, Afternoon, Dusk, Night }

    public const float SUNRISE_TIME = 0.25f;
    public const float NOON_TIME    = 0.5f;
    public const float SUNSET_TIME  = 0.75f;
    public const float CURFEW_TIME  = 0.833f;

    public event Action OnSunrise;
    public event Action OnSunset;
    public event Action OnCurfew;
    public event Action OnMidnight;
    public event Action<DayPhase> OnPhaseChanged;

    [Header("Time")]
    public float dayDurationSeconds = 300f;
    [Range(0f, 1f)] public float startingTimeOfDay = 0.3f;
    public bool pauseTime = false;

    [Header("Sun")]
    public Light sunLight;
    public Color sunriseSunsetColor = new Color(1f, 0.6f, 0.3f);
    public Color middayColor        = new Color(1f, 0.98f, 0.9f);
    public Color nightColor         = new Color(0.1f, 0.12f, 0.2f);
    public float maxSunIntensity    = 1.3f;
    public float minSunIntensity    = 0.05f;

    [Header("Ambient Audio")]
    public AudioClip dayAmbient;
    public AudioClip nightAmbient;
    public float ambientFadeDuration = 6f;

    [Header("Debug")]
    public bool showDebugTime = false;

    public float    TimeOfDay    { get; private set; }
    public int      DayNumber    { get; private set; } = 1;
    public DayPhase CurrentPhase { get; private set; } = DayPhase.Morning;

    public bool IsCurfewActive => TimeOfDay >= CURFEW_TIME || TimeOfDay < SUNRISE_TIME;
    public bool IsNight        => TimeOfDay >= SUNSET_TIME  || TimeOfDay < SUNRISE_TIME;

    private bool _sunriseEventFired;
    private bool _sunsetEventFired;
    private bool _curfewEventFired;
    private bool _midnightEventFired;

    void Start()
    {
        TimeOfDay    = startingTimeOfDay;
        CurrentPhase = GetPhaseForTime(TimeOfDay);

        if (sunLight == null)
            sunLight = FindFirstObjectByType<Light>();

        if (AudioManager.Instance != null && dayAmbient != null)
            AudioManager.Instance.PlayAmbient(dayAmbient);

        ApplySunSettings();
    }

    void Update()
    {
        if (pauseTime) return;

        TimeOfDay = (TimeOfDay + Time.deltaTime / dayDurationSeconds) % 1f;

        ApplySunSettings();
        CheckPhaseTransitions();
        FireThresholdEvents();

        // if (showDebugTime)
        //     Debug.Log($"[DayNightCycle] Day {DayNumber} | {GetFormattedTime()} | {CurrentPhase}");
    }

    void ApplySunSettings()
    {
        if (sunLight == null) return;

        sunLight.transform.rotation =
            Quaternion.Euler((TimeOfDay - 0.25f) * 360f, -30f, 0f);

        float intensityT = Mathf.Clamp01(Mathf.Sin(TimeOfDay * Mathf.PI));
        sunLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, intensityT);

        Color targetColor;
        if (TimeOfDay < SUNRISE_TIME || TimeOfDay > SUNSET_TIME)
            targetColor = nightColor;
        else if (TimeOfDay < SUNRISE_TIME + 0.05f || TimeOfDay > SUNSET_TIME - 0.05f)
            targetColor = sunriseSunsetColor;
        else
            targetColor = Color.Lerp(sunriseSunsetColor, middayColor,
                Mathf.InverseLerp(SUNRISE_TIME + 0.05f, NOON_TIME, TimeOfDay));

        sunLight.color = Color.Lerp(sunLight.color, targetColor, Time.deltaTime * 2f);
        RenderSettings.ambientIntensity = Mathf.Lerp(0.1f, 1.0f, intensityT);
    }

    void CheckPhaseTransitions()
    {
        DayPhase newPhase = GetPhaseForTime(TimeOfDay);
        if (newPhase == CurrentPhase) return;
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(CurrentPhase);

        if (CurrentPhase == DayPhase.Morning && AudioManager.Instance != null && dayAmbient != null)
            AudioManager.Instance.CrossfadeAmbient(dayAmbient, ambientFadeDuration);
        if (CurrentPhase == DayPhase.Night && AudioManager.Instance != null && nightAmbient != null)
            AudioManager.Instance.CrossfadeAmbient(nightAmbient, ambientFadeDuration);
    }

    DayPhase GetPhaseForTime(float t)
    {
        if (t >= SUNRISE_TIME && t < NOON_TIME)   return DayPhase.Morning;
        if (t >= NOON_TIME    && t < SUNSET_TIME) return DayPhase.Afternoon;
        if (t >= SUNSET_TIME  && t < CURFEW_TIME) return DayPhase.Dusk;
        return DayPhase.Night;
    }

    void FireThresholdEvents()
    {
        if (!_sunriseEventFired && TimeOfDay >= SUNRISE_TIME && TimeOfDay < SUNRISE_TIME + 0.01f)
        { _sunriseEventFired = true; OnSunrise?.Invoke(); }

        if (!_sunsetEventFired && TimeOfDay >= SUNSET_TIME && TimeOfDay < SUNSET_TIME + 0.01f)
        { _sunsetEventFired = true; OnSunset?.Invoke(); }

        if (!_curfewEventFired && TimeOfDay >= CURFEW_TIME && TimeOfDay < CURFEW_TIME + 0.01f)
        { _curfewEventFired = true; OnCurfew?.Invoke(); }

        if (!_midnightEventFired && TimeOfDay >= 0.99f) _midnightEventFired = true;
        if (_midnightEventFired && TimeOfDay < 0.01f)
        {
            _midnightEventFired = false;
            _sunriseEventFired  = false;
            _sunsetEventFired   = false;
            _curfewEventFired   = false;
            DayNumber++;
            OnMidnight?.Invoke();
        }
    }

    public string GetFormattedTime()
    {
        float total = TimeOfDay * 24f * 60f;
        return $"{Mathf.FloorToInt(total / 60f) % 24:D2}:{Mathf.FloorToInt(total % 60f):D2}";
    }

    public void SkipToMorning()
    {
        TimeOfDay = SUNRISE_TIME + 0.02f;
        DayNumber++;
        OnMidnight?.Invoke();
        _sunriseEventFired  = true;
        _sunsetEventFired   = false;
        _curfewEventFired   = false;
        _midnightEventFired = false;
        OnSunrise?.Invoke();
        CurrentPhase = DayPhase.Morning;
        OnPhaseChanged?.Invoke(CurrentPhase);
        if (AudioManager.Instance != null && dayAmbient != null)
            AudioManager.Instance.CrossfadeAmbient(dayAmbient, ambientFadeDuration);
        ApplySunSettings();
        Debug.Log($"[DayNightCycle] Skipped to morning. Now Day {DayNumber}.");
    }

    public void LoadState(int dayNumber, float timeOfDay)
    {
        DayNumber = dayNumber;
        TimeOfDay = timeOfDay;

        _sunriseEventFired  = timeOfDay > SUNRISE_TIME;
        _sunsetEventFired   = timeOfDay > SUNSET_TIME;
        _curfewEventFired   = timeOfDay > CURFEW_TIME;
        _midnightEventFired = false;

        CurrentPhase = GetPhaseForTime(timeOfDay);
        ApplySunSettings();

        Debug.Log($"[DayNightCycle] Loaded - Day {DayNumber}, Time {GetFormattedTime()}");
    }

    public void AdvanceTime(float amount) => TimeOfDay = (TimeOfDay + amount) % 1f;
}
