using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerInputType { Player1_Space, Player2_Mouse }
    [Header("Player")]
    public PlayerInputType playerInput;

    [Header("Move")]
    public float moveSpeed = 7f;
    public int moveDir = 1;

    [Header("Jump")]
    public float jumpForce = 15f;
    public int maxJump = 2;
    private int jumpCount = 0;
    public float jumpBufferTime = 0.15f;
    public float coyoteTime = 0.15f;

    private float lastJumpPressedTime = -10f;
    private float lastGroundedTime = -10f;

    [Header("Obstacle")]
    public LayerMask obstacleLayer;
    public float checkDistance = 0.7f;
    public float checkHeight = 0.7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Wall Slide")]
    public float slideSpeed = 3f;
    public float wallSlideGravity = 1.5f;
    public float normalGravity = 4f;
    public float wallSlideSmooth = 10f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool touchingObstacle;
    private bool isWallSliding;
    private Vector2 lastWallNormal;

    // Debug flags
    private bool debugWallTop, debugWallMid, debugWallBot;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        RegisterInput();
    }

    void OnDestroy()
    {
        UnregisterInput();
    }

    void RegisterInput()
    {
        if (GlobalInput.Instance == null) return;
        if (playerInput == PlayerInputType.Player1_Space)
            GlobalInput.Instance.OnSpaceDown += OnJumpPressed;
        else if (playerInput == PlayerInputType.Player2_Mouse)
            GlobalInput.Instance.OnMouseDown += OnJumpPressed;
    }

    void UnregisterInput()
    {
        if (GlobalInput.Instance == null) return;
        if (playerInput == PlayerInputType.Player1_Space)
            GlobalInput.Instance.OnSpaceDown -= OnJumpPressed;
        else if (playerInput == PlayerInputType.Player2_Mouse)
            GlobalInput.Instance.OnMouseDown -= OnJumpPressed;
    }

    void OnJumpPressed() => lastJumpPressedTime = Time.time;

    void FixedUpdate()
    {
        CheckObstacles();
        HandleWallSlide();
        HandleJump();
        HandleMovement();
    }

    void CheckObstacles()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, obstacleLayer);
        if (isGrounded) lastGroundedTime = Time.time;

        float halfHeight = checkHeight / 2f;
        Vector2 top = new Vector2(transform.position.x, transform.position.y + halfHeight);
        Vector2 mid = transform.position;
        Vector2 bot = new Vector2(transform.position.x, transform.position.y - halfHeight);
        Vector2 dir = new Vector2(moveDir, 0);

        RaycastHit2D hitTop = Physics2D.Raycast(top, dir, checkDistance, obstacleLayer);
        RaycastHit2D hitMid = Physics2D.Raycast(mid, dir, checkDistance, obstacleLayer);
        RaycastHit2D hitBot = Physics2D.Raycast(bot, dir, checkDistance, obstacleLayer);

        touchingObstacle = hitTop || hitMid || hitBot;
        if (hitTop) lastWallNormal = hitTop.normal;
        else if (hitMid) lastWallNormal = hitMid.normal;
        else if (hitBot) lastWallNormal = hitBot.normal;

        // Debug flags
        debugWallTop = hitTop;
        debugWallMid = hitMid;
        debugWallBot = hitBot;

        //Debug.Log($"[CheckObstacles] Top:{hitTop.collider != null} Mid:{hitMid.collider != null} Bot:{hitBot.collider != null} Touching:{touchingObstacle}");
    }

    void HandleWallSlide()
    {
        if (!isGrounded && touchingObstacle && Mathf.Abs(lastWallNormal.x) > 0.9f)
        {
            // 使用左右碰撞判定
            float halfHeight = checkHeight / 2f;
            Vector2 top = new Vector2(transform.position.x, transform.position.y + halfHeight);
            Vector2 mid = transform.position;
            Vector2 bot = new Vector2(transform.position.x, transform.position.y - halfHeight);
            Vector2 dir = new Vector2(moveDir, 0);

            RaycastHit2D hitTop = Physics2D.Raycast(top, dir, checkDistance, obstacleLayer);
            RaycastHit2D hitMid = Physics2D.Raycast(mid, dir, checkDistance, obstacleLayer);
            RaycastHit2D hitBot = Physics2D.Raycast(bot, dir, checkDistance, obstacleLayer);

            bool stillTouchingWall = hitTop || hitMid || hitBot;

            if (stillTouchingWall)
            {
                isWallSliding = true;
                jumpCount = 0;
                //Debug.Log($"[WallSlide] Sliding ON, velocityY={rb.velocity.y}");
            }
            else
            {
                isWallSliding = false;
                //Debug.Log($"[WallSlide] Sliding OFF, velocityY={rb.velocity.y}");
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    void HandleMovement()
    {
        // 地面碰障碍翻转方向
        if (isGrounded && touchingObstacle)
        {
            moveDir *= -1;
        }

        if (isWallSliding)
        {
            rb.gravityScale = normalGravity * 1.5f; // 保持正常重力

            // 检测左右是否还有墙（微调 Raycast 避免角落卡）
            float halfHeight = checkHeight / 2f;
            Vector2 top = new Vector2(transform.position.x, transform.position.y + halfHeight);
            Vector2 mid = transform.position;
            Vector2 bot = new Vector2(transform.position.x, transform.position.y - halfHeight);
            Vector2 dir = new Vector2(moveDir, 0);

            RaycastHit2D hitTop = Physics2D.Raycast(top, dir, checkDistance, obstacleLayer);
            RaycastHit2D hitMid = Physics2D.Raycast(mid, dir, checkDistance, obstacleLayer);
            RaycastHit2D hitBot = Physics2D.Raycast(bot, dir, checkDistance, obstacleLayer);

            bool stillTouchingWall = hitTop || hitMid || hitBot;

            if (!stillTouchingWall)
            {
                // 左右没墙了，取消滑墙状态
                isWallSliding = false;

                // 保持原水平速度，让角色自然滑下角落
                rb.velocity = new Vector2(moveDir * 0.05f, rb.velocity.y);
                //Debug.Log("[WallSlide] No wall, free fall with small horizontal velocity");
            }
            else
            {
                // 左右还有墙，限制最大下落速度
                float limitedY = Mathf.Max(rb.velocity.y, -slideSpeed);
                // 水平速度微调，避免卡角
                float horizontalVelocity = moveDir * 0.1f;
                rb.velocity = new Vector2(horizontalVelocity, limitedY);
                //Debug.Log($"[WallSlide] Sliding, Y capped: {limitedY}, small horizontal: {horizontalVelocity}");
            }
        }
        else
        {
            // 普通移动
            rb.gravityScale = normalGravity;
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        }
    }

    void HandleJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool coyoteValid = Time.time - lastGroundedTime <= coyoteTime;

        if (!jumpBuffered) return;

        if (isWallSliding)
        {
            moveDir *= -1;
            rb.velocity = new Vector2(moveDir * moveSpeed, jumpForce);
            jumpCount = 1;
            lastJumpPressedTime = -10f;
            isWallSliding = false;
            //Debug.Log("[HandleJump] Wall Jump executed");
            return;
        }

        if (coyoteValid)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount = 1;
            lastJumpPressedTime = -10f;
            return;
        }

        if (jumpCount < maxJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            lastJumpPressedTime = -10f;
        }
    }

}