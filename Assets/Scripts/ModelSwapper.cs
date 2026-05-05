using UnityEngine;

public class ModelSwapper : MonoBehaviour
{
    [Tooltip("The finished model prefab exported from Blender.")]
    public GameObject finalModelPrefab;

    [Tooltip("If true, the placeholder MeshRenderer is disabled rather than deleted.")]
    public bool disablePlaceholderInsteadOfDelete = true;

    [Tooltip("Matches the instantiated model's transform to this GameObject automatically.")]
    public bool matchTransform = true;

    public void Swap()
    {
        if (finalModelPrefab == null)
        {
            Debug.LogWarning($"[ModelSwapper] No final model assigned on {gameObject.name}.");
            return;
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (disablePlaceholderInsteadOfDelete)
        {
            if (mr) mr.enabled = false;
        }
        else
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mr) Destroy(mr);
            if (mf) Destroy(mf);
        }

        GameObject model = Instantiate(finalModelPrefab, transform);

        if (matchTransform)
        {
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale    = Vector3.one;
        }

        model.name = $"{finalModelPrefab.name}_Model";

        Debug.Log($"[ModelSwapper] Swapped {gameObject.name} → {finalModelPrefab.name}");
    }
}
