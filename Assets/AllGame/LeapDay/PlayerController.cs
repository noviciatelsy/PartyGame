using UnityEngine;
using UnityEngine.Windows;

public enum PlayerType
{
    Player1,
    Player2
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public PlayerType player;
    [Header("Audio")]
    public AudioClip jumpClip;      // 跳跃音效
    private AudioSource audioSource; // 播放器

    [Header("Move")]
    public float moveSpeed = 6f;
    int moveDir = 1;

    [Header("Jump")]
    private float jumpForceY = 16f;       // 垂直跳跃速度
    public int maxJump = 2;
    int jumpCount = 0;

    [Header("Wall Slide")]
    private float slideSpeed = 2f; // 最大滑墙下落速度
    private float slideAcceleration = 3f; // 滑墙下落加速度

    Rigidbody2D rb;
    LayerMask wallLayer;
    LayerMask groundLayer;

    bool isWallSliding = false;
    bool wallLeft = false;
    bool wallRight = false;
    bool isGround = true;

    public ImageChange imagechange;
    public ImageChange imagechange1;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallLayer = LayerMask.GetMask("Wall"); // 确保墙体在 Wall Layer
        groundLayer = LayerMask.GetMask("Wall", "OnewayPlatform");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // 订阅输入
        SubscribeInput();
    }

    void SubscribeInput()
    {
        if (player == PlayerType.Player1)
            GlobalInput.Instance.OnMouseLeftAction += OnInput;
        else
            GlobalInput.Instance.OnSpaceAction += OnInput;
    }

    void UnsubscribeInput()
    {
        if (GlobalInput.Instance == null) return;

        if (player == PlayerType.Player1)
            GlobalInput.Instance.OnMouseLeftAction -= OnInput;
        else
            GlobalInput.Instance.OnSpaceAction -= OnInput;
    }
    private void OnDestroy()
    {
        UnsubscribeInput();
    }

    void OnInput(GlobalInput.InputType type)
    {
        if (type != GlobalInput.InputType.SingleClick)
            return;

        Jump();
    }

    void FixedUpdate()
    {
        // 检测前方墙体
        float detectDist = 0.9f;
        float detectDist2 = 0.8f;

        bool wallRighttop = Physics2D.Raycast(transform.position +new Vector3(0,0.75f,0), Vector2.right, detectDist, wallLayer);
        bool wallRightmid = Physics2D.Raycast(transform.position + new Vector3(0, 0f, 0), Vector2.right, detectDist, wallLayer);
        bool wallRightLow = Physics2D.Raycast(transform.position + new Vector3(0, -0.75f, 0), Vector2.right, detectDist, wallLayer);
        wallRight = (wallRighttop || wallRightmid || wallRightLow);
        bool wallLeftTop = Physics2D.Raycast(transform.position + new Vector3(0, 0.75f, 0), Vector2.left, detectDist, wallLayer);
        bool wallLeftMid = Physics2D.Raycast(transform.position + new Vector3(0, 0f, 0), Vector2.left, detectDist, wallLayer);
        bool wallLeftLow = Physics2D.Raycast(transform.position + new Vector3(0, -0.75f, 0), Vector2.left, detectDist, wallLayer);

        wallLeft = wallLeftTop || wallLeftMid || wallLeftLow;
        isGround = Physics2D.Raycast(transform.position, Vector2.down, detectDist2, groundLayer);

        //Debug.Log($"[FixedUpdate] velocity: ({rb.velocity.x:F2}, {rb.velocity.y:F2}), moveDir: {moveDir}, jumpCount: {jumpCount}, Wall Right: {wallRight}, Wall Left: {wallLeft}, isGround: {isGround}");

        // 刷新跳跃次数：碰到墙或踩地面
        if (isGround || wallLeft || wallRight)
        {
            jumpCount = 0;
        }

        // 计算墙滑状态
        isWallSliding = !isGround && (wallLeft || wallRight);

        // 水平移动逻辑
        float targetVelX = moveDir * moveSpeed;

        if (isWallSliding)
        {
            // 墙滑时只控制竖直速度
            float newVelY = rb.velocity.y - slideAcceleration * Time.fixedDeltaTime;
            newVelY = Mathf.Max(newVelY, -slideSpeed);
            rb.velocity = new Vector2(rb.velocity.x, newVelY);
        }
        else
        {
            // 普通移动
            rb.velocity = new Vector2(targetVelX, rb.velocity.y);
        }

        // 墙反向逻辑，仅在非墙滑状态下触发
        if (isGround && ((wallRight && moveDir > 0) || (wallLeft && moveDir < 0)))
        {
            moveDir *= -1;
            //Debug.Log("1");
        }

        //// 可视化射线，方便调试
        //Debug.DrawRay(transform.position, Vector2.right * detectDist, Color.red);
        //Debug.DrawRay(transform.position, Vector2.left * detectDist, Color.green);
        //Debug.DrawRay(transform.position, Vector2.down * detectDist2, Color.yellow);
        UpdateImage();
    }

    void Jump()
    {
        if (jumpCount >= maxJump) return;

        //Debug.Log($"[FixedUpdate] velocity: ({rb.velocity.x:F2}, {rb.velocity.y:F2}), moveDir: {moveDir}, jumpCount: {jumpCount}, Wall Right: {wallRight}, Wall Left: {wallLeft}, isGround: {isGround}");
        if (jumpClip != null && audioSource != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f); // 随机音高，防止重复感
            audioSource.PlayOneShot(jumpClip);
        }

        if (isWallSliding)
        {
            // 墙跳：离开墙体方向水平 + 垂直跳跃
            float jumpDir = wallRight ? -1 : 1; // 离开墙的方向
            rb.velocity = new Vector2(moveSpeed * jumpDir, jumpForceY);
        }
        else
        {   
            // 普通跳跃
            rb.velocity = new Vector2(moveSpeed * moveDir, jumpForceY);
        }

        if ((wallRight && moveDir > 0) || (wallLeft && moveDir < 0))
        {
            moveDir *= -1;
            //Debug.Log("2");
        }
        jumpCount++;
    }

    void UpdateImage()
    {
        // 滑墙状态
        if (isWallSliding)
        {
            imagechange.SetImageIndex(1);
            imagechange1.SetImageIndex(1);
            return;
        }

        // 跳跃状态（在空中但不滑墙）
        if (!isGround)
        {
            imagechange.SetImageIndex(0);
            imagechange1.SetImageIndex(0);
            return;
        }

        // 普通状态
        imagechange.SetImageIndex(2);
        imagechange1.SetImageIndex(2);
    }

    public void Towin()
    {
        Debug.LogWarning("win");
        if (player == PlayerType.Player1)
        {
            LeapdayGM.Instance.DeclareWinner(2);
        }
        else
        {
            LeapdayGM.Instance.DeclareWinner(1);
        }
    }

    public void ToUp()
    {
        Debug.LogWarning("Up");
        float upJumpForce = 28f;

        // 清除当前竖直速度再给一个大跳
        rb.velocity = new Vector2(rb.velocity.x, upJumpForce);
    }

    public void ToDown()
    {
        Debug.LogWarning("Down");
    }
}