using UnityEngine;

[CreateAssetMenu(fileName = "NewMedicine", menuName = "Little Apothecary/Medicine Data")]
public class MedicineData : ScriptableObject
{
    [Header("Identity")]
    public string medicineName = "Unknown Medicine";

    [TextArea(2, 3)]
    public string description = "";

    [Header("Visuals")]
    public Sprite icon;
    public Color  labelColor = new Color(0.8f, 0.9f, 0.7f);

    [Header("Recipe")]
    [Tooltip("Every herb ingredient required to craft one unit of this medicine.")]
    public MedicineIngredient[] ingredients;
}

[System.Serializable]
public class MedicineIngredient
{
    public HerbData herb;
    [Min(1)] public int quantity = 1;
}
