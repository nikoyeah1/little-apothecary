using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Tooltip("The transform the player is teleported to after punishment.")]
    public Transform palaceSpawnPoint;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void ReturnPlayerToPalace()
    {
        if (palaceSpawnPoint == null)
        {
            Debug.LogWarning("[SpawnManager] No palace spawn point assigned.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[SpawnManager] No Player found.");
            return;
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(
            palaceSpawnPoint.position,
            palaceSpawnPoint.rotation);

        if (cc != null) cc.enabled = true;

        Debug.Log("[SpawnManager] Player returned to palace.");
    }
}
