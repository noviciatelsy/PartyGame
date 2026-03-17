using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button chooseButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private PlayableDirector playableDirector;
    void Awake()
    {
        playableDirector.Play();
    }
    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => LevelManager.Instance.NextLevel());
        }

        if (chooseButton != null)
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => LevelManager.Instance.LoadLevel("ZZZCredits"));
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => Application.Quit());
        }
    }

}
