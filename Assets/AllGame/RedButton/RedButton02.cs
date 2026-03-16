using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RedButton02 : MonoBehaviour
{
    [Header("�κӲ���")]
    public float winThreshold = 12f;  // ���ﵽ����ʤ��
    public float powerPerClick = 1f;

    [Header("��")]
    public Click player1Hand;
    public Click player2Hand;

    public List<GameObject> ButtonPrefab1;
    public List<GameObject> ButtonPrefab2;

    [Header("按钮图片切换回弹延迟")]
    private float buttonResetDelay = 0.05f;

    [Header("��������")]
    public Transform lineObject;
    private const float progressToWorldScale = 40f;

    public float progress = 0.5f;
    private float targetProgress = 0.5f;
    public float smoothSpeed = 20f;

    public Material splitMaterial;
    private float tugPower = 0f;
    private bool gameFinished = false;
    private Coroutine winCoroutine;

    private Coroutine button1Coroutine;
    private Coroutine button2Coroutine;

    private void Awake()
    {
        progress = 0.5f;
        targetProgress = 0.5f;

        if (splitMaterial != null)
            splitMaterial.SetFloat("_LineOffset", 0.5f);

        lineObject.position = new Vector3(0, 0, 5);
    }

    private void Start()
    {
        player1Hand.OnPressed += () => HandleButtonPress(1);
        player2Hand.OnPressed += () => HandleButtonPress(2);
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;
        splitMaterial.SetFloat("_LineOffset", 0.5f);
        //UpdateUI();
    }

    private void Update()
    {
        // ÿ֡ƽ���ƽ�Ŀ��
        progress = Mathf.Lerp(progress, targetProgress, Time.deltaTime * smoothSpeed);

        SyncShader();
    }

    private void OnDestroy()
    {
        if (GlobalInput.Instance == null) return;

        GlobalInput.Instance.OnSpaceAction -= OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction -= OnPlayer2Input;
    }

    // ======================
    // Player 1
    // ======================
    private void OnPlayer1Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        player1Hand.Press();

        // ���?1������
        tugPower -= powerPerClick;

        AfterPowerChanged();
    }

    // ======================
    // Player 2
    // ======================
    private void OnPlayer2Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        player2Hand.Press();

        // ���?2������
        tugPower += powerPerClick;

        AfterPowerChanged();
    }

    private void AfterPowerChanged()
    {
        CheckWin();
        //UpdateUI();
        UpdateTargetProgress();
    }

    private void UpdateTargetProgress()
    {
        float normalized = tugPower / winThreshold;
        normalized = Mathf.Clamp(normalized, -1f, 1f);

        targetProgress = 0.5f - normalized * 0.4f;
        targetProgress = Mathf.Clamp01(targetProgress);
    }

    private void CheckWin()
    {
        if (Mathf.Abs(tugPower) >= winThreshold)
        {
            gameFinished = true;

            if (tugPower > 0)
                DeclareWinner(2);
            else
                DeclareWinner(1);
        }
    }

    private void DeclareWinner(int playerIndex)
    {
        Debug.Log("Winner is Player " + playerIndex);
        GlobalScoreManager.Instance.AddScore(playerIndex, 1);
        // �����������?
        // ʤ��UI
        // ����
        // ��Ч
        // �ؿ�����
        var camShake = Camera.main ? Camera.main.GetComponent<CameraEffects.CameraShake>() : null;
        if (camShake != null)
            StartCoroutine(camShake.Shake());
        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    private void SyncShader()
    {
        if (splitMaterial != null)
        {
            splitMaterial.SetFloat("_LineOffset", progress);
        }
        SyncLineObject();
    }

    private void SyncLineObject()
    {
        if (lineObject == null) return;

        float delta = progress - 0.5f;

        float xPos = delta * progressToWorldScale;

        lineObject.position = new Vector3(
            xPos,
            0f,
            5f
        );
    }

    private void UpdateUI()
    {
        //
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
        // 强制初始状�?
        //buttonPrefabs[1].SetActive(false);
        //buttonPrefabs[0].SetActive(true);

        buttonPrefabs[0].SetActive(false);
        buttonPrefabs[1].SetActive(true);

        yield return new WaitForSeconds(buttonResetDelay);

        buttonPrefabs[1].SetActive(false);
        buttonPrefabs[0].SetActive(true);
    }
}