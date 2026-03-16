using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
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
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallLayer = LayerMask.GetMask("Wall"); // 确保墙体在 Wall Layer
        groundLayer = LayerMask.GetMask("Wall", "OnewayPlatform");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // 检测前方墙体
        float detectDist = 0.9f;
        float detectDist2 = 0.8f;

        wallRight = Physics2D.Raycast(transform.position, Vector2.right, detectDist, wallLayer);
        wallLeft = Physics2D.Raycast(transform.position, Vector2.left, detectDist, wallLayer);
        isGround = Physics2D.Raycast(transform.position, Vector2.down, detectDist2, groundLayer);

        Debug.Log($"[FixedUpdate] velocity: ({rb.velocity.x:F2}, {rb.velocity.y:F2}), moveDir: {moveDir}, jumpCount: {jumpCount}, Wall Right: {wallRight}, Wall Left: {wallLeft}, isGround: {isGround}");

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
        }

        // 可视化射线，方便调试
        Debug.DrawRay(transform.position, Vector2.right * detectDist, Color.red);
        Debug.DrawRay(transform.position, Vector2.left * detectDist, Color.green);
        Debug.DrawRay(transform.position, Vector2.down * detectDist2, Color.yellow);
    }

    void Jump()
    {
        if (jumpCount >= maxJump) return;

        Debug.Log($"[FixedUpdate] velocity: ({rb.velocity.x:F2}, {rb.velocity.y:F2}), moveDir: {moveDir}, jumpCount: {jumpCount}, Wall Right: {wallRight}, Wall Left: {wallLeft}, isGround: {isGround}");

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
        }
        jumpCount++;
    }
}