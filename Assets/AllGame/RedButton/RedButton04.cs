using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedButton04 : MonoBehaviour
{
    [Header("Players")]
    public Click player1Hand;
    public Click player2Hand;

    public List<GameObject> ButtonPrefab1;
    public List<GameObject> ButtonPrefab2;

    [Header("按钮图片切换回弹延迟")]
    public float buttonResetDelay = 0.15f;

    private bool canTouch = false;
    private bool gameFinished = false;
    private Coroutine winCoroutine;

    private void Start()
    {
        player1Hand.OnPressed += () => HandleButtonPress(1);
        player2Hand.OnPressed += () => HandleButtonPress(2);
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
    // �غ�����
    // =========================
    private IEnumerator StartRound()
    {
        canTouch = false;
        gameFinished = false;

        yield return null;
    }


    // =========================
    // �������
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
    // �ж��߼�
    // =========================
    private void HandleClick(int playerIndex)
    {
        if (gameFinished) return;

        if (canTouch)
        {
            // ��ȷ�������ǰ��һ�ʤ
            DeclareWinner(playerIndex);
        }
        else
        {
            // ��ǰ�������ǰ���ʧ��
            int otherPlayer = playerIndex == 1 ? 2 : 1;
            DeclareWinner(otherPlayer);
        }
    }

    private void DeclareWinner(int playerIndex)
    {
        gameFinished = true;
        canTouch = false;

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

    private void HandleButtonPress(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                StartCoroutine(SwapButtonImage(ButtonPrefab1));
                break;
            case 2:
                StartCoroutine(SwapButtonImage(ButtonPrefab2));
                break;
        }
    }

    private IEnumerator SwapButtonImage(List<GameObject> buttonPrefabs)
    {
        buttonPrefabs[0].SetActive(false);
        buttonPrefabs[1].SetActive(true);
        yield return new WaitForSeconds(buttonResetDelay);
        buttonPrefabs[1].SetActive(false);
        buttonPrefabs[0].SetActive(true);
    }
}
