using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Audio")]
    [Tooltip("Music clip to play on the main menu.")]
    [SerializeField] private AudioClip menuMusicClip;

    void Start()
    {
        ShowMainPanel();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (AudioManager.Instance != null)
        {
            if (menuMusicClip != null)
            {
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlayMusic(menuMusicClip);
            }
            else
            {
                Debug.LogWarning("[MainMenu] menuMusicClip is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning("[MainMenu] AudioManager.Instance is null.");
        }
    }

    public void OnPlayPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
        else
            SceneManager.LoadScene("Game");
    }

    public void OnOptionsPressed()
    {
        ShowOptionsPanel();
    }

    public void OnBackPressed()
    {
        ShowMainPanel();
    }

    public void OnQuitPressed()
    {
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
        if (mainPanel)    mainPanel.SetActive(true);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    void ShowOptionsPanel()
    {
        if (mainPanel)    mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }
}
