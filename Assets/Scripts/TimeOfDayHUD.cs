using UnityEngine;
using TMPro;

public class TimeOfDayHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI dayNumberText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI phaseText;

    [Header("Colours")]
    public Color normalColor  = new Color(0.85f, 0.82f, 0.7f);
    public Color curfewColor  = new Color(0.9f, 0.35f, 0.25f);
    public Color nightColor   = new Color(0.5f, 0.6f, 0.85f);

    private DayNightCycle _dayNight;

    void Start()
    {
        _dayNight = FindFirstObjectByType<DayNightCycle>();

        if (_dayNight == null)
            Debug.LogWarning("[TimeOfDayHUD] No DayNightCycle found in scene.");
    }

    void Update()
    {
        if (_dayNight == null) return;

        if (dayNumberText != null)
            dayNumberText.text = $"Day {_dayNight.DayNumber}";

        if (timeText != null)
            timeText.text = _dayNight.GetFormattedTime();

        string phaseLabel = _dayNight.CurrentPhase switch
        {
            DayNightCycle.DayPhase.Morning   => "Morning",
            DayNightCycle.DayPhase.Afternoon => "Afternoon",
            DayNightCycle.DayPhase.Dusk      => "Dusk",
            DayNightCycle.DayPhase.Night     => "Night",
            _ => ""
        };

        if (phaseText != null)
            phaseText.text = phaseLabel;

        Color activeColor = _dayNight.IsCurfewActive ? curfewColor :
                            _dayNight.IsNight        ? nightColor  : normalColor;

        if (timeText      != null) timeText.color      = activeColor;
        if (phaseText     != null) phaseText.color     = activeColor;
        if (dayNumberText != null) dayNumberText.color = normalColor;
    }
}
