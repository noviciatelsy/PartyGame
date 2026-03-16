using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScoreManager : MonoBehaviour
{
    public static GlobalScoreManager Instance;
    public Action<int> OnGameEnd;
    private VictoryAnimation victoryAnim;
    [Header("Score")]
    public int player1Score = 0;
    public int player2Score = 0;
    [Header("Game Over Settings")]
    public int winTarget = 5;
    public string gameOverScene = "GameEnd";
    public float delayBeforeGameOver = 1.2f;


    private void Awake()
    {
        //Debug.Log($"[ScoreManager] Awake on {gameObject.name}  scene:{SceneManager.GetActiveScene().name}");

        if (Instance != null && Instance != this)
        {
            Debug.Log("[ScoreManager] Duplicate detected 锟斤拷 Destroying new one");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        //Debug.Log("[ScoreManager] Set as Instance + DontDestroyOnLoad");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnSceneChanging;
    }

    void Start()
    {
        //Debug.Log($"[ScoreManager] Start | Score: {player1Score}:{player2Score}");
        UpdateUI();
    }

    void OnDestroy()
    {
        //Debug.Log($"[ScoreManager] OnDestroy called on {gameObject.name}");

        if (Instance == this)
        {
            Debug.Log("[ScoreManager] Removing scene callbacks");

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnSceneChanging;
        }
    }

    void OnSceneChanging(Scene oldScene, Scene newScene)
    {
        //Debug.Log($"[ScoreManager] Scene Changing {oldScene.name} 锟斤拷 {newScene.name}");

        transform.SetParent(null);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"[ScoreManager] Scene Loaded: {scene.name}");

        BindToCamera();

        //Debug.Log($"[ScoreManager] Score After Load: {player1Score}:{player2Score}");

        UpdateUI();
        // 在每个场景加载后，尝试把当前分数反映到场景中的 VictoryAnimation（生成对应数量的奖杯）
        VictoryAnimation va = GetVictoryAnimation();
        if (va != null)
        {
            va.PopulateTrophiesFromScores(player1Score, player2Score);
        }
    }

    private Transform cameraTransform;
    void BindToCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.Log("[ScoreManager] Camera.main NOT FOUND");
            return;
        }

        //Debug.Log("[ScoreManager] Following Camera");

        cameraTransform = cam.transform;
    }
    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.position = cameraTransform.position;
            transform.rotation = cameraTransform.rotation;
        }
    }

    // ==========================
    // 锟解部锟斤拷锟矫接匡拷
    // ==========================

    public void AddScore(int playerID, int amount = 1)
    {
        Debug.Log($"[ScoreManager] AddScore Player{playerID} +{amount}");

        if (playerID == 1)
        {
            player1Score += amount;
            PlayVictoryAnimation(true);
        }
        else if (playerID == 2)
        {
            player2Score += amount;
            PlayVictoryAnimation(false);
        }

        Debug.Log($"[ScoreManager] Score Now: {player1Score}:{player2Score}");

        UpdateUI();

        // 检查是否达到胜利目标
        if (player1Score >= winTarget || player2Score >= winTarget)
        {
            StartCoroutine(DelayedGameOver(playerID));
        }
    }

    private IEnumerator DelayedGameOver(int winnerPlayerID)
    {
        yield return new WaitForSeconds(delayBeforeGameOver);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(gameOverScene);
            OnGameEnd?.Invoke(winnerPlayerID);
        }
    }

    public int GetScore(int playerID)
    {
        if (playerID == 1) return player1Score;
        if (playerID == 2) return player2Score;
        return 0;
    }

    public void ResetScore()
    {
        //Debug.Log("[ScoreManager] ResetScore CALLED");

        player1Score = 0;
        player2Score = 0;

        UpdateUI();
    }

    // ==========================
    // UI

    // ==========================

    public void UpdateUI()
    {

    }

    public void PlayVictoryAnimation(bool isPlayer1)
    {
        VictoryAnimation anim = GetVictoryAnimation();

        if (anim != null)
        {
            anim.PlayVictory(isPlayer1);
        }
    }

    VictoryAnimation GetVictoryAnimation()
    {
        if (victoryAnim == null)
        {
            victoryAnim = FindObjectOfType<VictoryAnimation>();

            if (victoryAnim == null)
                Debug.LogWarning("[ScoreManager] VictoryAnimation not found in scene!");
        }

        return victoryAnim;
    }
}