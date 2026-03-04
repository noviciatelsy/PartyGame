using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedButton04 : MonoBehaviour
{
    [Header("Players")]
    public Click player1Hand;
    public Click player2Hand;

    private bool canTouch = false;
    private bool gameFinished = false;
    private Coroutine winCoroutine;


    private void Start()
    {
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;

        StartCoroutine(StartRound());
    }

    private void OnDestroy()
    {
        if (GlobalInput.Instance == null) return;

        GlobalInput.Instance.OnSpaceAction -= OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction -= OnPlayer2Input;
    }


    // =========================
    // 回合流程
    // =========================
    private IEnumerator StartRound()
    {
        canTouch = false;
        gameFinished = false;

        yield return null;
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


        Debug.Log("Winner is Player " + playerIndex);

        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(2f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

}
