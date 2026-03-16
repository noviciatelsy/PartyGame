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

    public List<GameObject> ButtonPrefab1;
    public List<GameObject> ButtonPrefab2;

    [Header("绿色按钮按下图片")]
    public GameObject greenPressedButton1;
    public GameObject greenPressedButton2;

    [Header("绿色按钮未按下图片")]
    public GameObject greenButton1;
    public GameObject greenButton2;

    [Header("按钮图片切换回弹延迟")]
    public float buttonResetDelay = 0.15f;

    private bool canTouch = false;
    private bool gameFinished = false;
    private Coroutine winCoroutine;

    private float reactionTimer = 0f;
    private bool isTiming = false;

    private void Start()
    {
        player1Hand.OnPressed += () => HandleButtonPress(1);
        player2Hand.OnPressed += () => HandleButtonPress(2);
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;

        greenButton.position = new Vector3(0f, 0f, 1f);

        UpdateText("-- ms");

        if (greenPressedButton1 != null) greenPressedButton1.SetActive(false);
        if (greenPressedButton2 != null) greenPressedButton2.SetActive(false);
        if (greenButton1 != null) greenButton1.SetActive(false);
        if (greenButton2 != null) greenButton2.SetActive(false);

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
    // �غ�����
    // =========================
    private IEnumerator StartRound()
    {
        canTouch = false;
        gameFinished = false;

        // ��ʼ����ťλ�� z = 1
        if (greenButton != null)
        {
            Vector3 pos = greenButton.position;
            pos.z = 1f;
            greenButton.position = pos;
        }

        // ����ȴ� 1~3 ��
        float randomDelay = Random.Range(1f, 3f);
        yield return new WaitForSeconds(randomDelay);

        // 如果等待期间玩家抢跑，不再变绿
        if (gameFinished) yield break;

        // 绿色按钮出现时设置排序
        canTouch = true;
        reactionTimer = 0f;
        isTiming = true;

        if (greenButton != null)
        {
            greenButton.position = new Vector3(greenButton.position.x, greenButton.position.y, -1f);
        }

        // 变绿后隐藏红色按钮，显示每个玩家的绿色按钮
        foreach (var go in ButtonPrefab1) if (go != null) go.SetActive(false);
        foreach (var go in ButtonPrefab2) if (go != null) go.SetActive(false);
        if (greenButton1 != null) greenButton1.SetActive(true);
        if (greenButton2 != null) greenButton2.SetActive(true);
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
            // 绿灯后按下：按下者显示绿色按下图片，另一方保持绿色按钮不变
            ShowGreenPressed(playerIndex);
            DeclareWinner(playerIndex);
        }
        else
        {
            // 抢跑：抢跑者显示红色按下图片（保持），另一方不变
            ShowRedPressed(playerIndex);
            int otherPlayer = playerIndex == 1 ? 2 : 1;
            DeclareWinner(otherPlayer);
        }
    }

    private void ShowGreenPressed(int playerIndex)
    {
        if (playerIndex == 1)
        {
            if (greenButton1 != null) greenButton1.SetActive(false);
            if (greenPressedButton1 != null) greenPressedButton1.SetActive(true);
        }
        else
        {
            if (greenButton2 != null) greenButton2.SetActive(false);
            if (greenPressedButton2 != null) greenPressedButton2.SetActive(true);
        }
    }

    private void ShowRedPressed(int playerIndex)
    {
        // 抢跑：隐藏该玩家绿色按钮，显示红色按下
        if (playerIndex == 1)
        {
            if (greenButton1 != null) greenButton1.SetActive(false);
            if (greenPressedButton1 != null) greenPressedButton1.SetActive(false);
            ButtonPrefab1[0].SetActive(false);
            ButtonPrefab1[1].SetActive(true);
        }
        else
        {
            if (greenButton2 != null) greenButton2.SetActive(false);
            if (greenPressedButton2 != null) greenPressedButton2.SetActive(false);
            ButtonPrefab2[0].SetActive(false);
            ButtonPrefab2[1].SetActive(true);
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

    private Coroutine button1Coroutine;
    private Coroutine button2Coroutine;
    private void HandleButtonPress(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                //StartCoroutine(SwapButtonImage(ButtonPrefab1));
                if (button1Coroutine != null)
                    StopCoroutine(button1Coroutine);

                button1Coroutine = StartCoroutine(SwapButtonImage(ButtonPrefab1));
                break;
            case 2:
                //StartCoroutine(SwapButtonImage(ButtonPrefab2));
                if (button2Coroutine != null)
                    StopCoroutine(button2Coroutine);
                button2Coroutine = StartCoroutine(SwapButtonImage(ButtonPrefab2));
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
