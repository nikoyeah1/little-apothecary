using UnityEngine;

[CreateAssetMenu(fileName = "NewHerb", menuName = "Little Apothecary/Herb Data")]
public class HerbData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name shown in the HUD popup and inventory.")]
    public string herbName = "Unknown Herb";

    [Tooltip("Short description shown in the herb popup. Should hint at the herb's " +
             "zone and use without being a direct map marker. " +
             "E.g. 'A hardy plant found where the air grows thin and cold.'")]
    [TextArea(2, 4)]
    public string description = "";

    [Header("Gameplay")]
    [Tooltip("Weight added to the player's pack when this herb is collected.")]
    public float weight = 5f;

    [Tooltip("Which zone this herb primarily grows in. " +
             "Used by HerbSpawner and for lore consistency.")]
    public ZoneType nativeZone = ZoneType.None;

    [Header("Visuals")]
    [Tooltip("Icon shown in the inventory grid. Assign a Sprite asset.")]
    public Sprite inventoryIcon;

    [Tooltip("Tint color used in the inventory slot background. " +
             "Gives each herb a distinct visual identity at a glance.")]
    public Color slotColor = Color.green;

    [Header("Audio")]
    [Tooltip("Sound played when the player picks up this herb.")]
    public AudioClip pickupSound;
}
