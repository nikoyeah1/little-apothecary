using UnityEngine;
using System;

public enum ZoneType
{
    None,
    PalaceGrounds,
    Plains,
    Forest,
    RiverLake,
    Mountain
}

public class ZoneManager : MonoBehaviour
{

    public static ZoneManager Instance { get; private set; }

    public event Action<ZoneType, ZoneType> OnZoneChanged;

    [Header("Debug")]
    [Tooltip("Logs zone transitions to the console.")]
    public bool debugLog = true;

    public ZoneType CurrentZone { get; private set; } = ZoneType.None;
    public ZoneType PreviousZone { get; private set; } = ZoneType.None;

    private int _zoneEntryCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void EnterZone(ZoneType zone)
    {
        if (zone == CurrentZone) return;

        PreviousZone = CurrentZone;
        CurrentZone  = zone;

        if (debugLog)
            Debug.Log($"[ZoneManager] {PreviousZone} → {CurrentZone}");

        OnZoneChanged?.Invoke(PreviousZone, CurrentZone);
    }

    public void ExitZone(ZoneType zone)
    {
        if (CurrentZone != zone) return;

        PreviousZone = CurrentZone;
        CurrentZone  = ZoneType.None;

        if (debugLog)
            Debug.Log($"[ZoneManager] Exited {PreviousZone} -> None");

        OnZoneChanged?.Invoke(PreviousZone, CurrentZone);
    }

    public bool IsInZone(ZoneType zone) => CurrentZone == zone;

    public bool IsOutdoors() => CurrentZone != ZoneType.PalaceGrounds && CurrentZone != ZoneType.None;
}
