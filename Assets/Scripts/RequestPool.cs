using System.Collections.Generic;
using UnityEngine;

public class RequestPool : MonoBehaviour
{

    [Header("Pool")]
    [Tooltip("All RequestData assets in the game.")]
    public RequestData[] allRequests;

    [Tooltip("How many requests should be active at the start of each day.")]
    public int dailyRequestCount = 3;

    private DayNightCycle _dayNight;

    void Start()
    {
        _dayNight = FindFirstObjectByType<DayNightCycle>();

        if (_dayNight != null)
            _dayNight.OnMidnight += IssueNewDayRequests;
        else
            Debug.LogError("[RequestPool] No DayNightCycle found.");

        IssueNewDayRequests();
    }

    void OnDestroy()
    {
        if (_dayNight != null)
            _dayNight.OnMidnight -= IssueNewDayRequests;
    }

    void IssueNewDayRequests()
    {
        if (RequestManager.Instance == null || allRequests == null) return;

        int activeCount = RequestManager.Instance.GetActiveRequests().Count;
        int slotsNeeded = dailyRequestCount - activeCount;

        if (slotsNeeded <= 0)
        {
            Debug.Log("[RequestPool] All request slots already filled by carry-overs.");
            return;
        }

        List<RequestData> candidates = BuildCandidateList();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[RequestPool] No eligible requests to issue.");
            return;
        }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int toIssue = Mathf.Min(slotsNeeded, candidates.Count);
        for (int i = 0; i < toIssue; i++)
            RequestManager.Instance.AddRequest(candidates[i]);

        Debug.Log($"[RequestPool] Issued {toIssue} new request(s). " +
                  $"Total active: {RequestManager.Instance.GetActiveRequests().Count}");
    }

    List<RequestData> BuildCandidateList()
    {
        HashSet<RequestData> activeData = new HashSet<RequestData>();
        foreach (ActiveRequest req in RequestManager.Instance.GetActiveRequests())
            activeData.Add(req.data);

        List<RequestData> candidates = new List<RequestData>();
        foreach (RequestData data in allRequests)
        {
            if (data != null && !activeData.Contains(data))
                candidates.Add(data);
        }

        return candidates;
    }
}
