using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePause : MonoBehaviour
{
    public static GamePause instance;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Image Panel;
    [SerializeField] private Animator panelAnimator;
    public bool IsGamePaused { get; private set; } = false;
    private bool isAnimating = false;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }
    void Start()
    {
        GlobalInput.Instance.OnEscapeDown += () =>
        {
            if (IsDisabledScene()) return;
            if (isAnimating) return;
            if (IsGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        };
        Panel.gameObject.SetActive(false);
        panelAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        resumeButton.onClick.AddListener(ResumeGame);
        quitButton.onClick.AddListener(QuitGame);
        MainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }
    private bool IsDisabledScene()
    {
        return SceneManager.GetActiveScene().name == "BeginScene";
    }
    private void PauseGame()
    {
        Time.timeScale = 0f;
        IsGamePaused = true;
        isAnimating = true;
        SetPanelInteractable(false);
        Panel.gameObject.SetActive(true);
        panelAnimator.Play("PauseShow", 0, 0f);
        StartCoroutine(WaitForAnimation(() =>
        {
            isAnimating = false;
            SetPanelInteractable(true);
        }));
    }
    private void ResumeGame()
    {
        isAnimating = true;
        SetPanelInteractable(false);
        panelAnimator.Play("ResumeHide", 0, 0f);
        StartCoroutine(WaitForAnimation(() =>
        {
            Panel.gameObject.SetActive(false);
            IsGamePaused = false;
            isAnimating = false;
            Time.timeScale = 1f;
        }));
    }

    private IEnumerator WaitForAnimation(System.Action onComplete)
    {
        yield return null;
        while (panelAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    // // 保留供 Animation Event 备用
    // public void OnPauseAnimationComplete()
    // {
    //     isAnimating = false;
    //     SetPanelInteractable(true);
    // }
    // public void OnResumeAnimationComplete()
    // {
    //     Panel.gameObject.SetActive(false);
    //     IsGamePaused = false;
    //     isAnimating = false;
    //     Time.timeScale = 1f;
    // }

    private void SetPanelInteractable(bool interactable)
    {
        resumeButton.interactable = interactable;
        quitButton.interactable = interactable;
        MainMenuButton.interactable = interactable;
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
