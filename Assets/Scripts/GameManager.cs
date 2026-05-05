using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; } = false;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName     = "Game";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == gameSceneName)
                TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else          PauseGame();
    }

    public void PauseGame()
    {
        IsPaused         = true;
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void ResumeGame()
    {
        IsPaused         = false;
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public void StartNewGame()
    {
        IsPaused       = false;
        Time.timeScale = 1f;

        AudioManager.Instance?.FadeMusic(0f, 0.5f);
        AudioManager.Instance?.StopAmbient();

        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToMainMenu()
    {
        IsPaused       = false;
        Time.timeScale = 1f;

        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.StopAmbient();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
