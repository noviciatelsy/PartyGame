using System.Collections;
using TMPro;
using UnityEngine;

public class CowboyFight : MonoBehaviour
{

    [Header("Countdown")]
    public float countdownTime = 3f;
    public TextMeshPro countdownText;

    private bool canShoot = false;
    public SpriteRenderer shootImage;
    private bool gameFinished = false;

    private bool isTiming = false;
    private float reactionTimer = 0f;

    public Cowboy player1;
    public Cowboy player2;

    private Coroutine winCoroutine;

    private Coroutine introMoveCoroutine;
    public Transform image1;  
    public Transform image2;

    void Start()
    {
        GlobalInput.Instance.OnSpaceAction += OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction += OnPlayer2Input;

        if (shootImage != null)
        {
            Color c = shootImage.color;
            c.a = 0f;
            shootImage.color = c;
        }

        StartCoroutine(StartDuel());
    }

    void OnDestroy()
    {
        if (GlobalInput.Instance == null) return;

        GlobalInput.Instance.OnSpaceAction -= OnPlayer1Input;
        GlobalInput.Instance.OnMouseLeftAction -= OnPlayer2Input;
    }

    void Update()
    {
        if (!isTiming) return;

        reactionTimer += Time.deltaTime;

    }

    // =========================
    // 开始对决
    // =========================
    IEnumerator StartDuel()
    {
        canShoot = false;
        gameFinished = false;

        if (countdownText != null)
            countdownText.text = "";

        yield return new WaitForSeconds(0.5f);

        // 启动角色移动协程
        if (introMoveCoroutine == null)
        {
            introMoveCoroutine = StartCoroutine(ImageIntroMove());
        }

        // 第1秒（不显示）
        yield return new WaitForSeconds(0.5f);


        if (countdownText != null)
            countdownText.text = "3";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "2";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "1";

        yield return new WaitForSeconds(1f);

        // 开枪
        if (countdownText != null)
            countdownText.text = "开火!";

        canShoot = true;
        isTiming = true;
        reactionTimer = 0f;

        Debug.Log("FIRE!");
    }

    // =========================
    // 输入监听
    // =========================
    void OnPlayer1Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        //player1Hand.Press();
        HandleShoot(1);
    }

    void OnPlayer2Input(GlobalInput.InputType type)
    {
        if (gameFinished) return;
        if (type != GlobalInput.InputType.SingleClick) return;

        Debug.Log("2");
        //player2Hand.Press();
        HandleShoot(2);
    }

    // =========================
    // 判定
    // =========================
    void HandleShoot(int playerIndex)
    {
        if (gameFinished) return;

        if (canShoot)
        {
            // 谁先开枪谁赢
            if (shootImage != null)
            {
                Color c = shootImage.color;
                c.a = 1f;
                shootImage.color = c;
            }
            if (playerIndex == 1)
            {
                player1.SetWin();
                player2.SetLose();
            }
            else
            {
                player2.SetWin();
                player1.SetLose();
            }
            DeclareWinner(playerIndex);
        }
        else
        {
            // 提前开枪判输
            int otherPlayer = playerIndex == 1 ? 2 : 1;
            DeclareWinner(otherPlayer);
        }
    }

    void DeclareWinner(int playerIndex)
    {
        gameFinished = true;
        canShoot = false;
        isTiming = false;

        Debug.Log("Winner is Player " + playerIndex);
        GlobalScoreManager.Instance.AddScore(playerIndex, 1);

        if (winCoroutine == null)
            winCoroutine = StartCoroutine(WinDelayCoroutine());
    }

    IEnumerator WinDelayCoroutine()
    {
        yield return new WaitForSeconds(3f);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }

    // =========================
    // 图片入场动画
    // =========================
    IEnumerator ImageIntroMove()
    {
        float duration = 0.5f;
        float timer = 0f;

        Vector3 image1Start = new Vector3(0f, 15f, 20f);
        Vector3 image1End = new Vector3(0f, 8.5f, 20f);

        Vector3 image2Start = new Vector3(0f, -15f, 20f);
        Vector3 image2End = new Vector3(0f, -9f, 20f);

        // 先设置初始位置
        if (image1 != null)
            image1.position = image1Start;

        if (image2 != null)
            image2.position = image2Start;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            if (image1 != null)
                image1.position = Vector3.Lerp(image1Start, image1End, t);

            if (image2 != null)
                image2.position = Vector3.Lerp(image2Start, image2End, t);

            yield return null;
        }

        // 保证最终精确位置
        if (image1 != null)
            image1.position = image1End;

        if (image2 != null)
            image2.position = image2End;
    }
}