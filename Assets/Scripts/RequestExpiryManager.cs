using UnityEngine;

public class RequestExpiryManager : MonoBehaviour
{
    [Header("Punishment")]
    public float punishmentDisplayDuration = 4f;

    [Header("References")]
    public PunishmentHUD punishmentHUD;

    private DayNightCycle _dayNight;
    private bool          _punishmentThisDayFired = false;
    private int           _expiredCount           = 0;

    void Start()
    {
        _dayNight = FindFirstObjectByType<DayNightCycle>();
        if (_dayNight == null) { Debug.LogError("[RequestExpiryManager] No DayNightCycle."); return; }
        _dayNight.OnCurfew   += HandleCurfew;
        _dayNight.OnMidnight += HandleNewDay;
    }

    void OnDestroy()
    {
        if (_dayNight == null) return;
        _dayNight.OnCurfew   -= HandleCurfew;
        _dayNight.OnMidnight -= HandleNewDay;
    }

    void HandleCurfew()
    {
        if (RequestManager.Instance == null) return;

        _expiredCount = 0;
        foreach (ActiveRequest req in RequestManager.Instance.GetActiveRequests())
            if (!req.IsComplete) _expiredCount++;

        if (_expiredCount > 0 && !_punishmentThisDayFired)
        {
            _punishmentThisDayFired = true;

            RequestManager.Instance.ClearExpiredRequests();

            TriggerPunishment(_expiredCount, PunishmentReason.MissedRequests);
        }
    }

    public void TriggerCurfewBreach()
    {
        if (_punishmentThisDayFired) return;
        _punishmentThisDayFired = true;
        TriggerPunishment(0, PunishmentReason.CurfewBreach);
    }

    void TriggerPunishment(int expiredRequests, PunishmentReason reason)
    {
        if (punishmentHUD != null)
            punishmentHUD.Show(reason, expiredRequests,
                               punishmentDisplayDuration, OnPunishmentComplete);
        else
            OnPunishmentComplete();
    }

    void OnPunishmentComplete()
    {
        SpawnManager.Instance?.ReturnPlayerToPalace();
        _dayNight?.SkipToMorning();

        SaveManager.Instance?.Save(0);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        player?.SetCursorLocked(true);
    }

    void HandleNewDay()
    {
        _punishmentThisDayFired = false;
    }
}

public enum PunishmentReason { MissedRequests, CurfewBreach }
