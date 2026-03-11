using System.Collections;
using TMPro;
using UnityEngine;

public class RedButton02 : MonoBehaviour
{
    [Header("拔河参数")]
    public float winThreshold = 12f;  // 差距达到多少胜利
    public float powerPerClick = 1f;

    [Header("手")]
    public Click player1Hand;
    public Click player2Hand;

    [Header("中线物体")]
    public Transform lineObject;
    private const float progressToWorldScale = 40f;

    public float progress = 0.5f;
    private float targetProgress = 0.5f;
    public float smoothSpeed = 20f;

    public Material splitMaterial;
    private float tugPower = 0f;
    private bool gameFinished = false;
    private Coroutine winCoroutine;

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
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;
        splitMaterial.SetFloat("_LineOffset", 0.5f);
        //UpdateUI();
    }

    private void Update()
    {
        // 每帧平滑逼近目标
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

        // 玩家1向右拉
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

        // 玩家2向左拉
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
        // 这里可以做：
        // 胜利UI
        // 动画
        // 音效
        // 关卡结束
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
}