using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingController : MonoBehaviour
{
    [Header("Volumes")]
    public Volume dayVolume;
    public Volume duskVolume;
    public Volume nightVolume;

    [Header("Transition Speed")]
    [Tooltip("How quickly volumes blend between phases.")]
    public float blendSpeed = 0.5f;

    private DayNightCycle _dayNight;

    void Start()
    {
        _dayNight = FindFirstObjectByType<DayNightCycle>();

        if (_dayNight == null)
            Debug.LogWarning("[PostProcessingController] No DayNightCycle found.");

        if (dayVolume)  dayVolume.weight  = 1f;
        if (duskVolume) duskVolume.weight = 0f;
        if (nightVolume) nightVolume.weight = 0f;
    }

    void Update()
    {
        if (_dayNight == null) return;

        float t = _dayNight.TimeOfDay;

        float targetDay  = 0f;
        float targetDusk = 0f;
        float targetNight = 0f;

        if (_dayNight.CurrentPhase == DayNightCycle.DayPhase.Morning ||
            _dayNight.CurrentPhase == DayNightCycle.DayPhase.Afternoon)
        {
            targetDay = 1f;
        }
        else if (_dayNight.CurrentPhase == DayNightCycle.DayPhase.Dusk)
        {
            float duskProgress = Mathf.InverseLerp(0.75f, 0.833f, t);
            targetDay  = 1f - duskProgress;
            targetDusk = duskProgress;
        }
        else
        {
            float nightProgress = Mathf.InverseLerp(0.833f, 0.88f, t);
            targetDusk  = 1f - nightProgress;
            targetNight = nightProgress;
        }

        float speed = blendSpeed * Time.deltaTime;
        if (dayVolume)   dayVolume.weight   = Mathf.MoveTowards(dayVolume.weight,   targetDay,   speed);
        if (duskVolume)  duskVolume.weight  = Mathf.MoveTowards(duskVolume.weight,  targetDusk,  speed);
        if (nightVolume) nightVolume.weight = Mathf.MoveTowards(nightVolume.weight, targetNight, speed);
    }
}
