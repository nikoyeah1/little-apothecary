using UnityEngine;

[CreateAssetMenu(fileName = "NewRequest", menuName = "Little Apothecary/Request Data")]
public class RequestData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Short title shown on the request board, e.g. 'For the Head Cook'")]
    public string requestTitle = "Palace Request";

    [TextArea(2, 4)]
    [Tooltip("Flavour text describing who needs the medicine and why.")]
    public string flavourText = "";

    [Header("Order")]
    [Tooltip("The medicine that must be crafted and delivered.")]
    public MedicineData requiredMedicine;

    [Tooltip("How many units of the medicine are needed.")]
    [Min(1)] public int quantity = 1;
}
