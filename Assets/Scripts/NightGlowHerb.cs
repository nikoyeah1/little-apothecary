using UnityEngine;

public class NightGlowHerb : MonoBehaviour
{

    [Header("Glow Settings")]
    [Tooltip("The emission colour at full night glow.")]
    public Color glowColor = new Color(0.3f, 0.8f, 0.4f);

    [Tooltip("Maximum emission intensity at full night.")]
    public float maxGlowIntensity = 1.5f;

    [Tooltip("How quickly the glow fades in/out (units per second).")]
    public float transitionSpeed = 1.5f;

    [Tooltip("Pulse speed.")]
    public float pulseSpeed = 1.2f;

    [Tooltip("Pulse magnitude (0 = no pulse, 0.3 = gentle throb).")]
    [Range(0f, 1f)] public float pulseMagnitude = 0.25f;

    private MeshRenderer  _meshRenderer;
    private Material      _materialInstance;
    private DayNightCycle _dayNight;
    private float         _currentIntensity = 0f;

    private static readonly int EmissionColorProperty =
        Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (_meshRenderer == null)
        {
            Debug.LogWarning($"[NightGlowHerb] No MeshRenderer on {gameObject.name}.");
            enabled = false;
            return;
        }

        _materialInstance = _meshRenderer.material;

        _materialInstance.EnableKeyword("_EMISSION");

        _dayNight = FindFirstObjectByType<DayNightCycle>();
    }

    void Update()
    {
        bool isNight = _dayNight != null ? _dayNight.IsNight : false;

        float targetIntensity = isNight ? maxGlowIntensity : 0f;

        _currentIntensity = Mathf.MoveTowards(
            _currentIntensity, targetIntensity,
            transitionSpeed * Time.deltaTime);

        float displayIntensity = _currentIntensity;
        if (_currentIntensity > 0.01f && pulseSpeed > 0f)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;
            displayIntensity *= pulse;
        }

        _materialInstance.SetColor(
            EmissionColorProperty,
            glowColor * displayIntensity);
    }

    void OnDestroy()
    {
        if (_materialInstance != null)
            Destroy(_materialInstance);
    }
}
