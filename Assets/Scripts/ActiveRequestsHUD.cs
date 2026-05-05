using UnityEngine;
using TMPro;

public class ActiveRequestsHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject      requestsWidget;
    [SerializeField] private TextMeshProUGUI summaryText;

    void Start()
    {
        if (RequestManager.Instance != null)
            RequestManager.Instance.OnRequestsChanged += Refresh;

        if (PalaceStorage.Instance != null)
            PalaceStorage.Instance.OnStorageChanged += Refresh;

        Refresh();
    }

    void OnDestroy()
    {
        if (RequestManager.Instance != null)
            RequestManager.Instance.OnRequestsChanged -= Refresh;

        if (PalaceStorage.Instance != null)
            PalaceStorage.Instance.OnStorageChanged -= Refresh;
    }

    void Refresh()
    {
        if (summaryText == null || RequestManager.Instance == null) return;

        var active = RequestManager.Instance.GetActiveRequests();

        if (active.Count == 0)
        {
            summaryText.text = RequestManager.Instance.GetCompletedRequests().Count > 0
                ? "<color=#7FC87F>All orders fulfilled!</color>"
                : "<color=#888888>No active orders.</color>";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (ActiveRequest req in active)
        {
            sb.Append($"<b>{req.data.requestTitle}</b>\n");

            if (req.data.requiredMedicine != null)
            {
                foreach (MedicineIngredient ing in req.data.requiredMedicine.ingredients)
                {
                    int stored = PalaceStorage.Instance?.GetQuantity(ing.herb) ?? 0;
                    bool ok    = stored >= ing.quantity;
                    string col = ok ? "#7FC87F" : "#C87F7F";
                    string sym = ok ? "V" : "X";
                    sb.Append($"  <color={col}>{ing.herb.herbName} ×{ing.quantity} {sym}</color>\n");
                }
            }

            sb.Append("\n");
        }

        summaryText.text = sb.ToString().TrimEnd();
    }
}
