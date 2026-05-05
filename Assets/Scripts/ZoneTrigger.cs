using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [Tooltip("Which zone this collider represents.")]
    public ZoneType zone = ZoneType.None;

    [Tooltip("Audio clip that plays as ambient when the player is in this zone. ")]
    public AudioClip dayAmbientClip;

    [Tooltip("Night ambient clip for this zone.")]
    public AudioClip nightAmbientClip;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ZoneManager.Instance != null)
            ZoneManager.Instance.EnterZone(zone);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ZoneManager.Instance != null)
            ZoneManager.Instance.ExitZone(zone);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = GetZoneColor(zone) * new Color(1, 1, 1, 0.15f);

        if (TryGetComponent<BoxCollider>(out var box))
            Gizmos.DrawCube(transform.position + box.center, box.size);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = GetZoneColor(zone);

        if (TryGetComponent<BoxCollider>(out var box))
            Gizmos.DrawWireCube(transform.position + box.center, box.size);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position, zone.ToString());
#endif
    }

    static Color GetZoneColor(ZoneType z)
    {
        return z switch
        {
            ZoneType.PalaceGrounds => Color.yellow,
            ZoneType.Plains        => Color.green,
            ZoneType.Forest        => new Color(0.1f, 0.5f, 0.1f),
            ZoneType.RiverLake     => Color.cyan,
            ZoneType.Mountain      => Color.gray,
            _                      => Color.white
        };
    }
}
