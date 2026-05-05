using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;

    [Header("Value Labels")]
    [SerializeField] private TextMeshProUGUI masterLabel;
    [SerializeField] private TextMeshProUGUI musicLabel;
    [SerializeField] private TextMeshProUGUI sfxLabel;
    [SerializeField] private TextMeshProUGUI ambientLabel;

    [Header("Display")]
    [SerializeField] private Toggle fogToggle;

    private const string KEY_MASTER  = "Vol_Master";
    private const string KEY_MUSIC   = "Vol_Music";
    private const string KEY_SFX     = "Vol_SFX";
    private const string KEY_AMBIENT = "Vol_Ambient";
    private const string KEY_FOG     = "Display_Fog";

    private bool _listenersAttached = false;

    void Start()
    {
        optionsPanel?.SetActive(false);
        LoadAndApplySettings();
    }

    public void Open()
    {
        optionsPanel?.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        RefreshSliderValues();

        SetupSliderListeners();

        if (masterSlider  == null) Debug.LogError("[OptionsMenu] MasterSlider not assigned.");
        if (musicSlider   == null) Debug.LogError("[OptionsMenu] MusicSlider not assigned.");
        if (sfxSlider     == null) Debug.LogError("[OptionsMenu] SFXSlider not assigned.");
        if (ambientSlider == null) Debug.LogError("[OptionsMenu] AmbientSlider not assigned.");
    }

    public void Close()
    {
        optionsPanel?.SetActive(false);
        SaveSettings();

        if (GameManager.Instance != null && !GameManager.Instance.IsPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void LoadAndApplySettings()
    {
        float master  = PlayerPrefs.GetFloat(KEY_MASTER,  1f);
        float music   = PlayerPrefs.GetFloat(KEY_MUSIC,   0.65f);
        float sfx     = PlayerPrefs.GetFloat(KEY_SFX,     1f);
        float ambient = PlayerPrefs.GetFloat(KEY_AMBIENT, 0.55f);
        bool  fog     = PlayerPrefs.GetInt(KEY_FOG, 1) == 1;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(master);
            AudioManager.Instance.SetMusicVolume(music);
            AudioManager.Instance.SetSFXVolume(sfx);
            AudioManager.Instance.SetAmbientVolume(ambient);
        }

        RenderSettings.fog = fog;
    }

    void RefreshSliderValues()
    {
        if (masterSlider)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.value = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        }
        if (musicSlider)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = PlayerPrefs.GetFloat(KEY_MUSIC, 0.65f);
        }
        if (sfxSlider)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        }
        if (ambientSlider)
        {
            ambientSlider.onValueChanged.RemoveAllListeners();
            ambientSlider.value = PlayerPrefs.GetFloat(KEY_AMBIENT, 0.55f);
        }
        if (fogToggle)
            fogToggle.isOn = PlayerPrefs.GetInt(KEY_FOG, 1) == 1;

        UpdateLabels();
    }

    void SetupSliderListeners()
    {
        if (masterSlider)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(v =>
            {
                AudioManager.Instance?.SetMasterVolume(v);
                UpdateLabels();
            });
        }

        if (musicSlider)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(v =>
            {
                AudioManager.Instance?.SetMusicVolume(v);
                UpdateLabels();
            });
        }

        if (sfxSlider)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(v =>
            {
                AudioManager.Instance?.SetSFXVolume(v);
                UpdateLabels();
            });
        }

        if (ambientSlider)
        {
            ambientSlider.onValueChanged.RemoveAllListeners();
            ambientSlider.onValueChanged.AddListener(v =>
            {
                AudioManager.Instance?.SetAmbientVolume(v);
                UpdateLabels();
            });
        }

        if (fogToggle)
        {
            fogToggle.onValueChanged.RemoveAllListeners();
            fogToggle.onValueChanged.AddListener(v => RenderSettings.fog = v);
        }
    }

    void UpdateLabels()
    {
        if (masterLabel  && masterSlider)
            masterLabel.text  = $"{Mathf.RoundToInt(masterSlider.value  * 100)}%";
        if (musicLabel   && musicSlider)
            musicLabel.text   = $"{Mathf.RoundToInt(musicSlider.value   * 100)}%";
        if (sfxLabel     && sfxSlider)
            sfxLabel.text     = $"{Mathf.RoundToInt(sfxSlider.value     * 100)}%";
        if (ambientLabel && ambientSlider)
            ambientLabel.text = $"{Mathf.RoundToInt(ambientSlider.value * 100)}%";
    }

    void SaveSettings()
    {
        if (masterSlider)  PlayerPrefs.SetFloat(KEY_MASTER,  masterSlider.value);
        if (musicSlider)   PlayerPrefs.SetFloat(KEY_MUSIC,   musicSlider.value);
        if (sfxSlider)     PlayerPrefs.SetFloat(KEY_SFX,     sfxSlider.value);
        if (ambientSlider) PlayerPrefs.SetFloat(KEY_AMBIENT, ambientSlider.value);
        if (fogToggle)     PlayerPrefs.SetInt(KEY_FOG, fogToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
