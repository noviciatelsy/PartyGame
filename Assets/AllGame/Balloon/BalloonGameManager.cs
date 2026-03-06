using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BalloonGameManager : MonoBehaviour
{
    public static BalloonGameManager instance;

    [Header("Player Settings")]
    public BalloonEntity p1;
    public List<GameObject> p1Medals;
    public BalloonEntity p2;
    public List<GameObject> p2Medals;

    [Header("Global Settings")]
    public float continuousSpeed = 0.8f;
    public float deflateSpeed = 0.2f;
    [SerializeField] private float initialTimer = 10.0f;
    private float currentTimer;

    [SerializeField] private float maxHoldTime;
    [SerializeField] private TextMeshProUGUI gameTip;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    private bool roundActive = false;
    private Coroutine winCoroutine; 

    private void Awake()
    {
        if (instance == null) instance = this;
        InitPlayer(p1);
        InitPlayer(p2);
        foreach (var medal in p1Medals) medal.SetActive(false);
        foreach (var medal in p2Medals) medal.SetActive(false);
        StartNewRound();
    }

    private void InitPlayer(BalloonEntity p)
    {
        p.anim = p.balloonObj.GetComponent<Animator>();
        p.spriteRenderer = p.balloonObj.GetComponent<SpriteRenderer>();
        p.minScale = p.balloonObj.transform.localScale;
    }

    private void StartNewRound()
    {
        maxHoldTime = Random.Range(5.0f, 8.0f);
        currentTimer = initialTimer;
        ResetBalloon(p1);
        ResetBalloon(p2);
        roundActive = true;
        gameTip.text = "游戏开始！";
        UpdateScoreTextOnly();
    }

    private void ResetBalloon(BalloonEntity p)
    {
        p.isGameOver = false;
        p.canContinuousInflate = false;
        p.isWaitingToPlayDeflateAnim = false;
        p.currentHoldTimer = 0;
        p.balloonObj.transform.localScale = p.minScale;
        if (p.spriteRenderer != null) p.spriteRenderer.sprite = p.initialSprite;
        p.anim.Play("Idle");
        if (p.prepareRoutine != null) StopCoroutine(p.prepareRoutine);
    }

    private void Update()
    {
        if (!roundActive) return;
        currentTimer -= Time.deltaTime;
        timerText.text = $"剩余：{Mathf.Max(0, currentTimer):F1}s";

        if (currentTimer <= 0)
        {
            EndRoundByTime();
            return;
        }

        HandlePlayerInput(p1, KeyCode.Space);
        HandlePlayerInput(p2, KeyCode.Mouse0);
    }

    private void HandlePlayerInput(BalloonEntity p, KeyCode key)
    {
        if (p.isGameOver) return;

        if (Input.GetKeyDown(key))
        {
            // 判定：只有在气球接近初始尺寸时，才允许触发“充充爆”起始动画
            if (p.balloonObj.transform.localScale.x <= p.minScale.x + 0.01f)
            {
                p.isWaitingToPlayDeflateAnim = false;
                if (p.prepareRoutine != null) StopCoroutine(p.prepareRoutine);
                p.prepareRoutine = StartCoroutine(PrepareToInflateRoutine(p, key));
            }
            else
            {
                // 如果气球还没漏完气就按下了，直接接管缩放逻辑，不重播动画
                p.canContinuousInflate = true;
                p.isWaitingToPlayDeflateAnim = false;
            }
        }

        if (Input.GetKey(key) && p.canContinuousInflate)
        {
            HandleContinuousInflation(p);
        }
        else if (!Input.GetKey(key))
        {
            HandleDeflation(p);
        }

        if (Input.GetKeyUp(key))
        {
            StopInflating(p);
        }
    }

    private IEnumerator PrepareToInflateRoutine(BalloonEntity p, KeyCode key)
    {
        p.canContinuousInflate = false;
        p.anim.Play("充充爆", 0, 0f);
        float waitTime = (p.ccb != null) ? p.ccb.length : 0.3f;
        yield return new WaitForSeconds(waitTime);
        if (Input.GetKey(key)) p.canContinuousInflate = true;
        else p.isWaitingToPlayDeflateAnim = true;
    }

    private void HandleContinuousInflation(BalloonEntity p)
    {
        if (p.spriteRenderer != null && p.spriteRenderer.sprite != p.inflatingSprite)
            p.spriteRenderer.sprite = p.inflatingSprite;

        p.currentHoldTimer += Time.deltaTime;
        float growth = continuousSpeed * Time.deltaTime;
        p.balloonObj.transform.localScale += new Vector3(growth, growth, growth);

        if (p.currentHoldTimer >= maxHoldTime) EndRoundByPop(p);
    }

    private void HandleDeflation(BalloonEntity p)
    {
        if (p.balloonObj.transform.localScale.x > p.minScale.x)
        {
            float shrink = deflateSpeed * Time.deltaTime;
            p.balloonObj.transform.localScale -= new Vector3(shrink, shrink, shrink);
            p.currentHoldTimer -= Time.deltaTime * (deflateSpeed / continuousSpeed);
            if (p.currentHoldTimer < 0) p.currentHoldTimer = 0;
        }
        else
        {
            if (p.spriteRenderer != null) p.spriteRenderer.sprite = p.initialSprite;

            if (p.isWaitingToPlayDeflateAnim)
            {
                p.anim.Play("爆爆充");
                p.isWaitingToPlayDeflateAnim = false;
            }
            p.balloonObj.transform.localScale = p.minScale;
            p.currentHoldTimer = 0;
        }
    }

    private void StopInflating(BalloonEntity p)
    {
        p.canContinuousInflate = false;
        if (p.prepareRoutine != null) StopCoroutine(p.prepareRoutine);
        if (!p.isGameOver)
        {
            if (p.balloonObj.transform.localScale.x > p.minScale.x + 0.05f) p.isWaitingToPlayDeflateAnim = true;
            else p.anim.Play("Idle");
        }
    }

    private void EndRoundByPop(BalloonEntity poppedPlayer)
    {
        roundActive = false;
        poppedPlayer.isGameOver = true;
        poppedPlayer.anim.Play("爆炸");
        poppedPlayer.balloonObj.transform.localScale = poppedPlayer.minScale;
        BalloonEntity winner = (poppedPlayer == p1) ? p2 : p1;
        gameTip.text = $"{poppedPlayer.playerName} 爆炸! {winner.playerName} 胜利! 正在前往下一关...";
        // 直接进入下一关（延迟展示）
        if (winCoroutine != null) StopCoroutine(winCoroutine);
        winCoroutine = StartCoroutine(ProceedToNextLevelCoroutine(2.0f));
    }

    private void EndRoundByTime()
    {
        roundActive = false;
        float p1Diff = maxHoldTime - p1.currentHoldTimer;
        float p2Diff = maxHoldTime - p2.currentHoldTimer;
        BalloonEntity winner = (p1Diff < p2Diff) ? p1 : p2;
        gameTip.text = $"时间到！{winner.playerName} 更接近极限，胜出！ 正在前往下一关...";
        if (winCoroutine != null) StopCoroutine(winCoroutine);
        winCoroutine = StartCoroutine(ProceedToNextLevelCoroutine(2.0f));
    }

    private void CheckMatchWinner()
    {
        // 旧的多轮逻辑已废弃 in 一局一胜 模式。
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return null;
    }

    private IEnumerator ProceedToNextLevelCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

    private void UpdateScoreUI()
    {
        if (p1.score > 0 && p1.score <= p1Medals.Count) p1Medals[p1.score - 1].SetActive(true);
        if (p2.score > 0 && p2.score <= p2Medals.Count) p2Medals[p2.score - 1].SetActive(true);
        UpdateScoreTextOnly();
    }

    private void UpdateScoreTextOnly()
    {
        scoreText.text = $"{p1.playerName}: {p1.score} | {p2.playerName}: {p2.score}";
    }
}