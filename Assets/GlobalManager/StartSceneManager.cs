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
        GamePause.instance.OpenPanelButton.gameObject.SetActive(false);
        playableDirector.Play();
    }
    public void Initialize()
    {
        startButton.onClick.AddListener(() => LevelManager.Instance.NextLevel());
        chooseButton.onClick.AddListener(() => LevelManager.Instance.LoadLevel("ChooseScene"));
        exitButton.onClick.AddListener(() => Application.Quit());
    }

}
