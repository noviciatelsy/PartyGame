using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedButton01 : MonoBehaviour
{
    public int targetClickCount = 5;

    public Click player1Hand;
    public Click player2Hand;
    public TextMeshPro player1Text;
    public TextMeshPro player2Text;
    public List<GameObject> ButtonPrefab1;
    public List<GameObject> ButtonPrefab2;

    private int player1Count = 0;
    private int player2Count = 0;

    private bool gameFinished = false;
    private Coroutine winCoroutine;

    [Header("按钮图片切换回弹延迟")]
    public float buttonResetDelay = 0.15f;

    private void Start()
    {
        player1Hand.OnPressed += () => HandleButtonPress(1);
        player2Hand.OnPressed += () => HandleButtonPress(2);
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;
    }

    private void OnDestroy()
    {
        if (GlobalInput.Instance == null) return;

        GlobalInput.Instance.OnSpaceAction -= OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction -= OnPlayer2Input;
    }

    // ======================
    // Player 1 (Space)
    // ======================
    private void OnPlayer1Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        player1Hand.Press();
        player1Count++;
        UpdateUI();

        Debug.Log("Player 1 Click: " + player1Count);

        if (player1Count >= targetClickCount)
        {
            DeclareWinner(1);
        }
    }

    // ======================
    // Player 2 (Mouse)
    // ======================
    private void OnPlayer2Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        player2Hand.Press();
        player2Count++;
        UpdateUI();

        Debug.Log("Player 2 Click: " + player2Count);

        if (player2Count >= targetClickCount)
        {
            DeclareWinner(2);
        }
    }

    private void DeclareWinner(int playerIndex)
    {
        gameFinished = true;

        Debug.Log("Winner is Player " + playerIndex);
        GlobalScoreManager.Instance.AddScore(playerIndex, 1);
        // ������Լӣ�
        // UI��ʾʤ��
        // ���Ŷ���
        // ��ֹ����
        // ������һ����
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private void UpdateUI()
    {
        if (player1Text != null)
            player1Text.text = player1Count.ToString();

        if (player2Text != null)
            player2Text.text = player2Count.ToString();
    }

    private IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3.0f);

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
