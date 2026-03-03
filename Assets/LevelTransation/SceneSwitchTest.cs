using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchTest : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string scene1Name;
    [SerializeField] private string scene2Name;

    public Animator Transition;
    void Awake()
    {
        Transition = GetComponentInChildren<Animator>();
        Debug.Log(Transition);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!string.IsNullOrEmpty(scene1Name))
            {
                StartCoroutine(LoadLevel(scene1Name));
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!string.IsNullOrEmpty(scene2Name))
            {
                StartCoroutine(LoadLevel(scene2Name));
            }
        }
    }

    IEnumerator LoadLevel(string sceneName)
    {
        Transition.SetTrigger("Start");
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene(sceneName);
    }
}
