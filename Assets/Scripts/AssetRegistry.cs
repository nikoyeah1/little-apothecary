using System.Collections.Generic;
using UnityEngine;

public class AssetRegistry : MonoBehaviour
{
    public static AssetRegistry Instance { get; private set; }

    [Header("Herb Assets")]
    [Tooltip("Every HerbData ScriptableObject in the project.")]
    public HerbData[] allHerbs;

    [Header("Request Assets")]
    [Tooltip("Every RequestData ScriptableObject in the project.")]
    public RequestData[] allRequests;

    private Dictionary<string, HerbData>    _herbMap    = new Dictionary<string, HerbData>();
    private Dictionary<string, RequestData> _requestMap = new Dictionary<string, RequestData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        foreach (HerbData h in allHerbs)
            if (h != null) _herbMap[h.name] = h;

        foreach (RequestData r in allRequests)
            if (r != null) _requestMap[r.name] = r;
    }

    public HerbData    GetHerb   (string assetName) =>
        _herbMap.TryGetValue(assetName, out var h) ? h : null;

    public RequestData GetRequest(string assetName) =>
        _requestMap.TryGetValue(assetName, out var r) ? r : null;
}
