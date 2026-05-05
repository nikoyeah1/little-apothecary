using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActiveRequest
{
    public RequestData data;
    public int         quantityFulfilled;
    public bool        IsComplete => quantityFulfilled >= data.quantity;
}

public class RequestManager : MonoBehaviour
{
    public static RequestManager Instance { get; private set; }

    public event Action OnRequestsChanged;

    [Tooltip("Leave empty when using RequestPool.")]
    public RequestData[] startingRequests;

    private List<ActiveRequest> _active    = new List<ActiveRequest>();
    private List<ActiveRequest> _completed = new List<ActiveRequest>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        RequestPool pool = FindFirstObjectByType<RequestPool>();
        if (pool == null && startingRequests != null)
        {
            foreach (RequestData data in startingRequests)
                if (data != null)
                    _active.Add(new ActiveRequest { data = data });
            OnRequestsChanged?.Invoke();
        }
    }

    public void AddRequest(RequestData data)
    {
        if (data == null) return;
        foreach (ActiveRequest e in _active)
            if (e.data == data) return;
        _active.Add(new ActiveRequest { data = data, quantityFulfilled = 0 });
        OnRequestsChanged?.Invoke();
    }

    public void AddRequestWithProgress(RequestData data, int fulfilled)
    {
        if (data == null) return;
        _active.Add(new ActiveRequest { data = data, quantityFulfilled = fulfilled });
        OnRequestsChanged?.Invoke();
    }

    public bool FulfillMedicine(MedicineData medicine)
    {
        bool anyAdvanced = false;
        foreach (ActiveRequest req in _active)
        {
            if (req.IsComplete || req.data.requiredMedicine != medicine) continue;
            req.quantityFulfilled++;
            anyAdvanced = true;
            if (req.IsComplete)
                Debug.Log($"[RequestManager] Complete: {req.data.requestTitle}");
            break;
        }

        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (_active[i].IsComplete)
            {
                _completed.Add(_active[i]);
                _active.RemoveAt(i);
            }
        }

        if (anyAdvanced) OnRequestsChanged?.Invoke();
        return anyAdvanced;
    }

    public void ClearExpiredRequests()
    {
        _active.RemoveAll(r => !r.IsComplete);
        OnRequestsChanged?.Invoke();
    }

    public void ClearCompletedRequests()
    {
        _completed.Clear();
        OnRequestsChanged?.Invoke();
    }

    public void ClearAll()
    {
        _active.Clear();
        _completed.Clear();
        OnRequestsChanged?.Invoke();
    }

    public IReadOnlyList<ActiveRequest> GetActiveRequests()    => _active.AsReadOnly();
    public IReadOnlyList<ActiveRequest> GetCompletedRequests() => _completed.AsReadOnly();
    public bool AllRequestsComplete() => _active.Count == 0 && _completed.Count > 0;
}
