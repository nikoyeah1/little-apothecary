using UnityEngine;

public class FootstepAudio : MonoBehaviour
{

    [Header("Clip Sets")]
    [Tooltip("Footstep sounds for grass / default terrain.")]
    public AudioClip[] grassFootsteps;

    [Tooltip("Footstep sounds for rocky / mountain terrain.")]
    public AudioClip[] rockFootsteps;

    [Tooltip("Footstep sounds for dirt paths.")]
    public AudioClip[] dirtFootsteps;

    [Header("Timing")]
    [Tooltip("Distance the player must travel between footsteps at normal speed.")]
    public float stepDistance = 2.2f;

    [Tooltip("Step distance when sprinting.")]
    public float sprintStepDistance = 1.6f;

    [Header("Volume")]
    [Tooltip("Base footstep volume.")]
    [Range(0f, 1f)] public float baseVolume = 0.5f;

    [Tooltip("Random volume variance applied per step.")]
    [Range(0f, 0.3f)] public float volumeVariance = 0.1f;

    [Header("Surface Detection")]
    [Tooltip("How far down to raycast to detect surface.")]
    public float groundRayLength = 2f;

    public LayerMask groundMask;

    private PlayerController _playerController;
    private Vector3          _lastStepPosition;
    private float            _distanceSinceLastStep;

    void Start()
    {
        _playerController  = GetComponent<PlayerController>();
        _lastStepPosition  = transform.position;
    }

    void Update()
    {
        if (_playerController == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        float moved = Vector3.Distance(transform.position, _lastStepPosition);
        _distanceSinceLastStep += moved;
        _lastStepPosition       = transform.position;

        float threshold = (_playerController.GetWeightRatio() < 0.5f)
            ? stepDistance : sprintStepDistance;

        // only step when grounded and actually moving
        if (_distanceSinceLastStep >= threshold && IsMoving())
        {
            _distanceSinceLastStep = 0f;
            PlayFootstep();
        }
    }

    void PlayFootstep()
    {
        AudioClip[] set = GetClipSetForSurface();
        if (set == null || set.Length == 0) return;

        AudioClip clip   = set[Random.Range(0, set.Length)];
        float     volume = baseVolume + Random.Range(-volumeVariance, volumeVariance);

        AudioManager.Instance?.PlaySFXAtPoint(clip, transform.position, volume);
    }

    AudioClip[] GetClipSetForSurface()
    {
        if (!Physics.Raycast(transform.position + Vector3.up * 0.1f,
                             Vector3.down, out RaycastHit hit,
                             groundRayLength, groundMask))
            return grassFootsteps;

        Terrain terrain = hit.collider.GetComponent<Terrain>();
        if (terrain != null)
        {
            int dominantLayer = GetDominantTerrainLayer(terrain, hit.point);
            return dominantLayer switch
            {
                1 => dirtFootsteps,
                2 => rockFootsteps,
                _ => grassFootsteps
            };
        }

        return grassFootsteps;
    }

    int GetDominantTerrainLayer(Terrain terrain, Vector3 worldPos)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = worldPos - terrain.transform.position;

        int mapX = Mathf.RoundToInt(terrainPos.x / data.size.x * data.alphamapWidth);
        int mapZ = Mathf.RoundToInt(terrainPos.z / data.size.z * data.alphamapHeight);

        mapX = Mathf.Clamp(mapX, 0, data.alphamapWidth  - 1);
        mapZ = Mathf.Clamp(mapZ, 0, data.alphamapHeight - 1);

        float[,,] alphas = data.GetAlphamaps(mapX, mapZ, 1, 1);

        int   dominant = 0;
        float max      = 0f;
        for (int i = 0; i < alphas.GetLength(2); i++)
        {
            if (alphas[0, 0, i] > max)
            {
                max      = alphas[0, 0, i];
                dominant = i;
            }
        }

        return dominant;
    }

    bool IsMoving()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return false;
        return kb.wKey.isPressed || kb.aKey.isPressed ||
               kb.sKey.isPressed || kb.dKey.isPressed;
    }
}
