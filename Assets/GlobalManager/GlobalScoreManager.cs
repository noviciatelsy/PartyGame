using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScoreManager : MonoBehaviour
{
    public static GlobalScoreManager Instance;

    private VictoryAnimation victoryAnim;
    [Header("Score")]
    public int player1Score = 0;
    public int player2Score = 0;


    private void Awake()
    {
        //Debug.Log($"[ScoreManager] Awake on {gameObject.name}  scene:{SceneManager.GetActiveScene().name}");

        if (Instance != null && Instance != this)
        {
            Debug.Log("[ScoreManager] Duplicate detected �� Destroying new one");
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
        //Debug.Log($"[ScoreManager] Scene Changing {oldScene.name} �� {newScene.name}");

        transform.SetParent(null);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"[ScoreManager] Scene Loaded: {scene.name}");

        BindToCamera();

        //Debug.Log($"[ScoreManager] Score After Load: {player1Score}:{player2Score}");

        UpdateUI();
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
    // �ⲿ���ýӿ�
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