using UnityEngine;

public class CurfewReturnDetector : MonoBehaviour
{
    private DayNightCycle        _dayNight;
    private RequestExpiryManager _expiryManager;

    void Start()
    {
        _dayNight      = FindFirstObjectByType<DayNightCycle>();
        _expiryManager = FindFirstObjectByType<RequestExpiryManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_dayNight == null || _expiryManager == null) return;

        bool returnedAfterMidnight = _dayNight.TimeOfDay < 0.25f &&
                                     _dayNight.TimeOfDay > 0.0f;

        if (returnedAfterMidnight)
        {
            Debug.Log("[CurfewReturnDetector] Player returned after midnight - punishment.");
            _expiryManager.TriggerCurfewBreach();
        }
    }
}
