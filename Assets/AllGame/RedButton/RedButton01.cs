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

    private float buttonResetDelay = 0.15f;
    float button1Timer = 0f;
    float button2Timer = 0f;
    public SpringButton spring1;
    public SpringButton spring2;

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


    private void Update()
    {
        if (button1Timer > 0)
        {
            button1Timer -= Time.deltaTime;

            if (button1Timer <= 0)
            {
                ButtonPrefab1[1].SetActive(false);
                ButtonPrefab1[0].SetActive(true);
            }
        }

        if (button2Timer > 0)
        {
            button2Timer -= Time.deltaTime;

            if (button2Timer <= 0)
            {
                ButtonPrefab2[1].SetActive(false);
                ButtonPrefab2[0].SetActive(true);
            }
        }
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

        var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
        if (camShake != null)
            StartCoroutine(camShake.Shake());
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

                button1Timer = buttonResetDelay;

                ButtonPrefab1[1].SetActive(false);
                ButtonPrefab1[0].SetActive(false);

                ButtonPrefab1[1].SetActive(true);
                if (spring1) spring1.Press();
                break;

            case 2:

                button2Timer = buttonResetDelay;

                ButtonPrefab2[1].SetActive(false);
                ButtonPrefab2[0].SetActive(false);

                ButtonPrefab2[1].SetActive(true);
                if (spring2) spring2.Press();
                break;
        }
    }
}
