using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("The root panel shown on first open.")]
    [SerializeField] private GameObject mainPanel;

    [Tooltip("Hidden by default. Shown when Options is pressed.")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Title Audio")]
    [Tooltip("Ambient or music clip to play on the main menu.")]
    [SerializeField] private AudioClip menuMusicClip;

    void Start()
    {
        ShowMainPanel();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (menuMusicClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(menuMusicClip);
    }

    public void OnPlayPressed()
    {
        PlayButtonClick();

        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
    public void OnOptionsPressed()
    {
        PlayButtonClick();
        ShowOptionsPanel();
    }

    public void OnBackPressed()
    {
        PlayButtonClick();
        ShowMainPanel();
    }

    public void OnQuitPressed()
    {
        PlayButtonClick();

        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void ShowMainPanel()
    {
        if (mainPanel)   mainPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    void ShowOptionsPanel()
    {
        if (mainPanel)   mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    void PlayButtonClick()
    {

    }
}
