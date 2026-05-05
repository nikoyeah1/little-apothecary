using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{

    public static SaveManager Instance { get; private set; }

    public const int SlotCount = 3;

    private float _playTimeSeconds = 0f;
    private bool  _isLoading       = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Update()
    {
        if (!_isLoading)
            _playTimeSeconds += Time.deltaTime;
    }

    public static string GetSavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"SaveSlot_{slot}.json");

    public static bool SlotExists(int slot) =>
        File.Exists(GetSavePath(slot));

    public bool Save(int slot)
    {
        try
        {
            SaveData data = BuildSaveData(slot);
            string   json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(GetSavePath(slot), json);

            Debug.Log($"[SaveManager] Saved to slot {slot}: {GetSavePath(slot)}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            return false;
        }
    }

    SaveData BuildSaveData(int slot)
    {
        SaveData data = new SaveData();

        data.saveSlot      = slot;
        data.saveDateTime  = DateTime.Now.ToString("yyyy-MM-dd  HH:mm");
        data.playTimeSeconds = Mathf.RoundToInt(_playTimeSeconds);

        DayNightCycle dayNight = FindFirstObjectByType<DayNightCycle>();
        if (dayNight != null)
        {
            data.dayNumber  = dayNight.DayNumber;
            data.timeOfDay  = dayNight.TimeOfDay;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition  = new SerializableVector3(player.transform.position);
            data.playerYRotation = player.transform.eulerAngles.y;

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) data.currentWeight = pc.currentWeight;

            Inventory inv = player.GetComponent<Inventory>();
            if (inv != null)
            {
                foreach (InventorySlot s in inv.GetSlots())
                    data.packContents.Add(new SavedHerbStack
                    {
                        herbAssetName = s.herb.name,
                        quantity      = s.quantity
                    });
            }
        }

        if (PalaceStorage.Instance != null)
        {
            foreach (var pair in PalaceStorage.Instance.GetAllStored())
                data.storageContents.Add(new SavedHerbStack
                {
                    herbAssetName = pair.Key.name,
                    quantity      = pair.Value
                });
        }

        if (RequestManager.Instance != null)
        {
            foreach (ActiveRequest req in RequestManager.Instance.GetActiveRequests())
                data.activeRequests.Add(new SavedRequest
                {
                    requestAssetName   = req.data.name,
                    quantityFulfilled  = req.quantityFulfilled
                });

            foreach (ActiveRequest req in RequestManager.Instance.GetCompletedRequests())
                data.completedRequests.Add(new SavedRequest
                {
                    requestAssetName  = req.data.name,
                    quantityFulfilled = req.quantityFulfilled
                });
        }

        RequestExpiryManager expiry = FindFirstObjectByType<RequestExpiryManager>();

        return data;
    }

    public bool Load(int slot)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] No save file at slot {slot}.");
            return false;
        }

        try
        {
            string   json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            _isLoading = true;
            ApplySaveData(data);
            _isLoading       = false;
            _playTimeSeconds = data.playTimeSeconds;

            Debug.Log($"[SaveManager] Loaded slot {slot} - Day {data.dayNumber}.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e.Message}");
            _isLoading = false;
            return false;
        }
    }

    void ApplySaveData(SaveData data)
    {
        AssetRegistry registry = AssetRegistry.Instance;
        if (registry == null)
        {
            Debug.LogError("[SaveManager] AssetRegistry not found - cannot restore assets.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && data.playerPosition != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.transform.SetPositionAndRotation(
                data.playerPosition.ToVector3(),
                Quaternion.Euler(0f, data.playerYRotation, 0f));
            if (cc) cc.enabled = true;

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.currentWeight = 0f;

            Inventory inv = player.GetComponent<Inventory>();
            if (inv != null)
            {
                inv.ClearAll();
                foreach (SavedHerbStack s in data.packContents)
                {
                    HerbData herb = registry.GetHerb(s.herbAssetName);
                    if (herb != null) inv.TryAddHerb(herb, s.quantity);
                    else Debug.LogWarning($"[SaveManager] Herb not found: {s.herbAssetName}");
                }
            }
        }

        if (PalaceStorage.Instance != null)
        {
            foreach (SavedHerbStack s in data.storageContents)
            {
                HerbData herb = registry.GetHerb(s.herbAssetName);
                if (herb != null) PalaceStorage.Instance.AddHerb(herb, s.quantity);
                else Debug.LogWarning($"[SaveManager] Herb not found: {s.herbAssetName}");
            }
        }

        DayNightCycle dayNight = FindFirstObjectByType<DayNightCycle>();
        if (dayNight != null)
            dayNight.LoadState(data.dayNumber, data.timeOfDay);

        if (RequestManager.Instance != null)
        {
            RequestManager.Instance.ClearAll();

            foreach (SavedRequest sr in data.activeRequests)
            {
                RequestData rd = registry.GetRequest(sr.requestAssetName);
                if (rd != null)
                    RequestManager.Instance.AddRequestWithProgress(rd, sr.quantityFulfilled);
                else Debug.LogWarning($"[SaveManager] Request not found: {sr.requestAssetName}");
            }
        }
    }

    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] Deleted save slot {slot}.");
        }
    }

    public SaveData PeekSlot(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch
        {
            return null;
        }
    }
}
