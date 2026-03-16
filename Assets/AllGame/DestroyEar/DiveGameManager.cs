using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Reflection;

public class DiveGameManager : MonoBehaviour
{
    public PlayerDive player1;
    public PlayerDive player2;
    public AudioSource deadMusicSource;
    public TMP_Text timerText;
    public float gameDuration = 15f;
    private float timer;
    public float startDelay = 3f;
    private float startTimer;

    private bool gameStarted = false;
    private bool gameFinished = false;
    private Coroutine winCoroutine;

    void Start()
    {
        timer = gameDuration;
        startTimer = startDelay;
    }

    void Update()
    {
        if (gameFinished) return;

        if (!gameStarted)
        {
            HandleStartDelay();
            return;
        }

        HandleInput();
        UpdateTimer();
    }

    void HandleStartDelay()
    {
        startTimer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = "准备";
        }

        if (startTimer <= 0f)
        {
            gameStarted = true;
        }
    }

    void HandleInput()
    {
        player1.SetHolding(GlobalInput.Instance.SpaceHolding);
        player2.SetHolding(GlobalInput.Instance.MouseHolding);
    }

    void UpdateTimer()
    {
        timer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = "倒计时:" + timer.ToString("F1") + "s";
        }

        if (timer <= 0f)
        {
            timer = 0f;
            EndGameCompare();
        }
    }

    public void PlayerFailed(PlayerDive player)
    {
        if (gameFinished) return;
        deadMusicSource?.Play();
        gameFinished = true;

        PlayerDive winner;

        if (player.playerType == PlayerDive.PlayerType.Player1)
        {
            Debug.Log("Player2 Wins!");
            winner = player2;
            GlobalScoreManager.Instance.AddScore(2, 1);
        }
        else
        {
            Debug.Log("Player1 Wins!");
            winner = player1;
            GlobalScoreManager.Instance.AddScore(1, 1);
        }

        player1.isfinished = true;
        player2.isfinished = true;
        // 停止输入
        player1.SetHolding(false);
        player2.SetHolding(false);

        // 胜利者淡出
        StartCoroutine(WinnerFadeRoutine(winner));
        var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
        if (camShake != null) StartCoroutine(camShake.ShakeWithFollow());
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    IEnumerator WinnerFadeRoutine(PlayerDive player)
    {
        SpriteRenderer depthSprite = player.depthSprite;

        float duration = 0.1f;
        float timer = 0f;

        Color startColor = depthSprite.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            depthSprite.color = c;

            yield return null;
        }
    }

    public void EndGameCompare()
    {
        if (gameFinished) return;

        float p1Depth = player1.GetDepth();
        float p2Depth = player2.GetDepth();

        float p1Distance = Mathf.Abs(p1Depth + 20f);
        float p2Distance = Mathf.Abs(p2Depth + 20f);

        if (p1Distance < p2Distance)
        {
            Debug.Log("Player1 Wins!");
            GlobalScoreManager.Instance.AddScore(1, 1);
        }
        else if (p2Distance < p1Distance)
        {
            Debug.Log("Player2 Wins!");
            GlobalScoreManager.Instance.AddScore(2, 1);
        }
        else
        {
            Debug.Log("Draw!");
        }
        player1.isfinished = true;
        player2.isfinished = true;
        StartCoroutine(WinnerFadeRoutine(player1));
        StartCoroutine(WinnerFadeRoutine(player2));
        gameFinished = true;

        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

}