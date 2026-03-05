using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScoreManager : MonoBehaviour
{
    public static GlobalScoreManager Instance;

    [Header("Score")]
    public int player1Score = 0;
    public int player2Score = 0;

    [Header("UI")]
    public TMP_Text player1Text;
    public TMP_Text player2Text;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        UpdateUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUI();
    }

    // ==========================
    // 外部调用接口
    // ==========================

    public void AddScore(int playerID, int amount = 1)
    {
        if (playerID == 1)
        {
            player1Score += amount;
        }
        else if (playerID == 2)
        {
            player2Score += amount;
        }

        UpdateUI();
    }

    public int GetScore(int playerID)
    {
        if (playerID == 1) return player1Score;
        if (playerID == 2) return player2Score;
        return 0;
    }

    public void ResetScore()
    {
        player1Score = 0;
        player2Score = 0;
        UpdateUI();
    }

    // ==========================
    // UI更新
    // ==========================

    public void UpdateUI()
    {
        Debug.Log("?");
        if (player1Text != null)
            player1Text.text = player1Score.ToString();

        if (player2Text != null)
            player2Text.text = player2Score.ToString();
    }
}
