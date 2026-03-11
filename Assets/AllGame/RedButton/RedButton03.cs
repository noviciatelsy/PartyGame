using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedButton03 : MonoBehaviour
{
    [Header("Green Button")]
    public Transform greenButton;

    [Header("Players")]
    public Click player1Hand;
    public Click player2Hand;
    public TextMeshPro player1Text;
    public TextMeshPro player2Text;

    private bool canTouch = false;
    private bool gameFinished = false;
    private Coroutine winCoroutine;

    private float reactionTimer = 0f;
    private bool isTiming = false;

    private void Start()
    {
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;

        greenButton.position = new Vector3(0f, 0f, 1f);

        UpdateText("-- ms");

        StartCoroutine(StartRound());
    }

    private void OnDestroy()
    {
        if (GlobalInput.Instance == null) return;

        GlobalInput.Instance.OnSpaceAction -= OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction -= OnPlayer2Input;
    }

    private void Update()
    {
        if (!isTiming) return;

        reactionTimer += Time.deltaTime;

        int ms = Mathf.FloorToInt(reactionTimer * 1000f);
        UpdateText(ms + " ms");
    }

    // =========================
    // 回合流程
    // =========================
    private IEnumerator StartRound()
    {
        canTouch = false;
        gameFinished = false;

        // 初始化按钮位置 z = 1
        if (greenButton != null)
        {
            Vector3 pos = greenButton.position;
            pos.z = 1f;
            greenButton.position = pos;
        }

        // 随机等待 1~3 秒
        float randomDelay = Random.Range(1f, 3f);
        yield return new WaitForSeconds(randomDelay);

        // 开始移动
        canTouch = true;
        reactionTimer = 0f;
        isTiming = true;

        //yield return StartCoroutine(MoveGreenButton());
        if (greenButton != null)
        {
            greenButton.position = new Vector3(greenButton.position.x, greenButton.position.y, -1f);
        }
    }


    // =========================
    // 玩家输入
    // =========================
    private void OnPlayer1Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        player1Hand.Press();
        HandleClick(1);
    }

    private void OnPlayer2Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        player2Hand.Press();
        HandleClick(2);
    }

    // =========================
    // 判定逻辑
    // =========================
    private void HandleClick(int playerIndex)
    {
        if (gameFinished) return;

        if (canTouch)
        {
            // 正确点击，当前玩家获胜
            DeclareWinner(playerIndex);
        }
        else
        {
            // 提前点击，当前玩家失败
            int otherPlayer = playerIndex == 1 ? 2 : 1;
            DeclareWinner(otherPlayer);
        }
    }

    private void DeclareWinner(int playerIndex)
    {
        gameFinished = true;
        canTouch = false;
        isTiming = false;

        Debug.Log("Winner is Player " + playerIndex);
        GlobalScoreManager.Instance.AddScore(playerIndex, 1);

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

    private void UpdateText(string value)
    {
        if (player1Text != null)
            player1Text.text = value;

        if (player2Text != null)
            player2Text.text = value;
    }
}
