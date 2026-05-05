using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private SaveLoadHUD _saveLoadHUD;
    private OptionsMenu _optionsMenu;

    void Start()
    {
        _saveLoadHUD = FindFirstObjectByType<SaveLoadHUD>(FindObjectsInactive.Include);
        _optionsMenu = FindFirstObjectByType<OptionsMenu>(FindObjectsInactive.Include);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        bool shouldShow = GameManager.Instance.IsPaused;
        if (pausePanel != null && pausePanel.activeSelf != shouldShow)
            pausePanel.SetActive(shouldShow);
    }

    public void OnResumePressed()    => GameManager.Instance?.ResumeGame();
    public void OnSavePressed()      => _saveLoadHUD?.OpenSave();
    public void OnLoadPressed()      => _saveLoadHUD?.OpenLoad();
    public void OnOptionsPressed()   => _optionsMenu?.Open();
    public void OnMainMenuPressed()  => GameManager.Instance?.ReturnToMainMenu();
    public void OnQuitPressed()      => GameManager.Instance?.QuitGame();
}
