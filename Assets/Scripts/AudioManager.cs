using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Looping background music source.")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("Looping ambient environment source.")]
    [SerializeField] private AudioSource ambientSource;

    [Tooltip("One-shot SFX source.")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Default Volumes (0-1)")]
    [Range(0f, 1f)] public float masterVolume  = 1f;
    [Range(0f, 1f)] public float musicVolume   = 0.65f;
    [Range(0f, 1f)] public float sfxVolume     = 1f;
    [Range(0f, 1f)] public float ambientVolume = 0.55f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource   == null) musicSource   = CreateSource("Music",   true,  false);
        if (ambientSource == null) ambientSource = CreateSource("Ambient", true,  false);
        if (sfxSource     == null) sfxSource     = CreateSource("SFX",     false, false);

        ApplyAllVolumes();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void FadeMusic(float targetVolume, float duration)
    {
        StopCoroutine(nameof(FadeSourceRoutine));
        StartCoroutine(FadeSourceRoutine(musicSource, targetVolume * musicVolume * masterVolume, duration));
    }

    public void CrossfadeMusic(AudioClip newClip, float duration)
    {
        StartCoroutine(CrossfadeRoutine(musicSource, newClip, duration));
    }

    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        ambientSource.clip = clip;
        ambientSource.loop = loop;
        ambientSource.Play();
    }

    public void StopAmbient() => ambientSource.Stop();

    public void CrossfadeAmbient(AudioClip newClip, float duration)
    {
        StartCoroutine(CrossfadeRoutine(ambientSource, newClip, duration));
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 worldPosition, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPosition, volumeScale * sfxVolume * masterVolume);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyAllVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume          = Mathf.Clamp01(value);
        musicSource.volume   = musicVolume * masterVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume          = Mathf.Clamp01(value);
        sfxSource.volume   = sfxVolume * masterVolume;
    }

    public void SetAmbientVolume(float value)
    {
        ambientVolume          = Mathf.Clamp01(value);
        ambientSource.volume   = ambientVolume * masterVolume;
    }

    void ApplyAllVolumes()
    {
        if (musicSource)   musicSource.volume   = musicVolume   * masterVolume;
        if (sfxSource)     sfxSource.volume     = sfxVolume     * masterVolume;
        if (ambientSource) ambientSource.volume = ambientVolume * masterVolume;
    }

    private IEnumerator FadeSourceRoutine(AudioSource source, float targetVolume, float duration)
    {
        float startVol = source.volume;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed      += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
        if (targetVolume <= 0f) source.Stop();
    }

    private IEnumerator CrossfadeRoutine(AudioSource source, AudioClip newClip, float duration)
    {
        float originalVolume = source.volume;
        float halfDuration   = duration * 0.5f;

        yield return FadeSourceRoutine(source, 0f, halfDuration);

        source.clip   = newClip;
        source.loop   = true;
        source.Play();

        yield return FadeSourceRoutine(source, originalVolume, halfDuration);
    }


    AudioSource CreateSource(string label, bool loop, bool playOnAwake)
    {
        GameObject go = new GameObject($"AudioSource_{label}");
        go.transform.SetParent(transform);
        AudioSource src      = go.AddComponent<AudioSource>();
        src.loop             = loop;
        src.playOnAwake      = playOnAwake;
        src.spatialBlend     = 0f;
        return src;
    }
}
