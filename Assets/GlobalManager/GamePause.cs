using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePause : MonoBehaviour
{
    public static GamePause instance;
    [SerializeField]private Button resumeButton;
    [SerializeField]private Button quitButton;
    [SerializeField]private Button MainMenuButton;
    [SerializeField] private Image Panel;
    public bool IsPaused { get; private set; } = false;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        GlobalInput.Instance.OnEscapeDown += () =>
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        };
        Panel.gameObject.SetActive(false);
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);
        MainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }
    private void PauseGame()
    {
        Time.timeScale = 0f;
        Panel.gameObject.SetActive(true);
        IsPaused = true;
    }
    private void ResumeGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        Panel.gameObject.SetActive(false);
    }
    private void QuitGame()
    {
        Application.Quit();
    }
    private void ReturnToMainMenu()
    {
        LevelManager.Instance.LoadLevel("MainMenu");
    }
}
