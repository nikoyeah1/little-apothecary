using System.Collections.Generic;
using UnityEngine;

public class HerbSpawner : MonoBehaviour
{

    [Header("Herb")]
    [Tooltip("The herb prefab to spawn. Must have a HerbPickup component.")]
    public GameObject herbPrefab;

    [Tooltip("The HerbData asset for this spawner's herb type. ")]
    public HerbData herbData;

    [Header("Spawn Points")]
    [Tooltip("Child GameObjects marking valid spawn positions.")]
    public Transform[] spawnPoints;

    [Header("Quantity")]
    [Tooltip("Minimum number of herbs to spawn each day.")]
    public int minPerDay = 2;

    [Tooltip("Maximum number of herbs to spawn each day.")]
    public int maxPerDay = 5;

    [Header("Behaviour")]
    [Tooltip("Destroy leftover herbs at dawn before spawning new ones.")]
    public bool clearStaleHerbsOnNewDay = true;

    [Tooltip("Height offset so herbs sit on top of terrain rather than clipping in.")]
    public float heightOffset = 0.15f;

    [Header("Layer")]
    [Tooltip("Name of the layer spawned herbs should be placed on. ")]
    public string interactableLayerName = "Interactable";

    private List<GameObject> _activeHerbs = new List<GameObject>();
    private DayNightCycle    _dayNight;
    private int              _interactableLayer;

    void Start()
    {
        _interactableLayer = LayerMask.NameToLayer(interactableLayerName);

        if (_interactableLayer == -1)
            Debug.LogWarning($"[HerbSpawner] Layer '{interactableLayerName}' not found. ");

        if (herbData == null)
            Debug.LogError($"[HerbSpawner] {gameObject.name} has no HerbData assigned. " +
                           $"Herbs will spawn as 'Unknown Herb' and cannot be identified.");

        _dayNight = FindFirstObjectByType<DayNightCycle>();
        if (_dayNight != null)
            _dayNight.OnSunrise += SpawnForNewDay;
        else
            Debug.LogWarning("[HerbSpawner] No DayNightCycle found - herbs won't respawn daily.");

        SpawnForNewDay();
    }

    void OnDestroy()
    {
        if (_dayNight != null)
            _dayNight.OnSunrise -= SpawnForNewDay;
    }

    void SpawnForNewDay()
    {
        if (herbPrefab == null)
        {
            Debug.LogWarning($"[HerbSpawner] {gameObject.name} has no herbPrefab assigned.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"[HerbSpawner] {gameObject.name} has no spawn points.");
            return;
        }

        if (clearStaleHerbsOnNewDay)
            ClearActiveHerbs();

        int count = Random.Range(minPerDay, Mathf.Min(maxPerDay, spawnPoints.Length) + 1);

        // shuffle spawn points
        List<Transform> shuffled = new List<Transform>(spawnPoints);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        for (int i = 0; i < count; i++)
        {
            Vector3    spawnPos = shuffled[i].position + Vector3.up * heightOffset;
            GameObject herb     = Instantiate(herbPrefab, spawnPos, shuffled[i].rotation);

            HerbPickup pickup = herb.GetComponentInChildren<HerbPickup>();
            if (pickup != null)
            {
                pickup.herbData = herbData;
            }
            else
            {
                Debug.LogWarning($"[HerbSpawner] Spawned herb has no HerbPickup component: " +
                                 $"{herb.name}. Check the prefab.");
            }

            if (_interactableLayer != -1)
                SetLayerRecursively(herb, _interactableLayer);

            _activeHerbs.Add(herb);
        }

        Debug.Log($"[HerbSpawner] {gameObject.name} ({herbData?.herbName ?? "?"}) " +
                  $"spawned {count} herb(s).");
    }

    void ClearActiveHerbs()
    {
        foreach (GameObject herb in _activeHerbs)
            if (herb != null) Destroy(herb);

        _activeHerbs.Clear();
    }

    static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
