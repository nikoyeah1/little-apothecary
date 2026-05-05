using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{

    [Header("Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Volume Sliders")]
    [SerializeField] private Slider          masterSlider;
    [SerializeField] private Slider          musicSlider;
    [SerializeField] private Slider          sfxSlider;
    [SerializeField] private Slider          ambientSlider;

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

    private bool _isOpen = false;

    void Start()
    {
        optionsPanel?.SetActive(false);
        LoadAndApplySettings();
        SetupSliderListeners();
    }

    public void Open()
    {
        _isOpen = true;
        optionsPanel?.SetActive(true);
        RefreshSliderValues();
    }

    public void Close()
    {
        _isOpen = false;
        optionsPanel?.SetActive(false);
        SaveSettings();
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

        RefreshSliderValues();
        if (fogToggle != null) fogToggle.isOn = fog;
    }

    void RefreshSliderValues()
    {
        if (masterSlider  != null) masterSlider.value  =
            PlayerPrefs.GetFloat(KEY_MASTER,  1f);
        if (musicSlider   != null) musicSlider.value   =
            PlayerPrefs.GetFloat(KEY_MUSIC,   0.65f);
        if (sfxSlider     != null) sfxSlider.value     =
            PlayerPrefs.GetFloat(KEY_SFX,     1f);
        if (ambientSlider != null) ambientSlider.value =
            PlayerPrefs.GetFloat(KEY_AMBIENT, 0.55f);

        UpdateLabels();
    }

    void SetupSliderListeners()
    {
        masterSlider? .onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetMasterVolume(v);
            UpdateLabels();
        });

        musicSlider?  .onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetMusicVolume(v);
            UpdateLabels();
        });

        sfxSlider?    .onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetSFXVolume(v);
            UpdateLabels();
        });

        ambientSlider?.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetAmbientVolume(v);
            UpdateLabels();
        });

        fogToggle?.onValueChanged.AddListener(v =>
        {
            RenderSettings.fog = v;
        });
    }

    void UpdateLabels()
    {
        if (masterLabel  != null && masterSlider  != null)
            masterLabel.text  = $"{Mathf.RoundToInt(masterSlider.value  * 100)}%";
        if (musicLabel   != null && musicSlider   != null)
            musicLabel.text   = $"{Mathf.RoundToInt(musicSlider.value   * 100)}%";
        if (sfxLabel     != null && sfxSlider     != null)
            sfxLabel.text     = $"{Mathf.RoundToInt(sfxSlider.value     * 100)}%";
        if (ambientLabel != null && ambientSlider != null)
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
