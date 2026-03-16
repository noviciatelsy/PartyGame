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
    private float checkDistance = 0.1f;
    private float checkHeight = 1.1f;
    private Transform groundCheck;
    private float groundCheckRadius = 0.15f;

    [Header("Wall Slide")]
    public float slideSpeed = 3f;
    public float normalGravity = 4f;

    private Rigidbody2D rb;

    private bool isGrounded;
    private bool touchingObstacle;
    private bool isWallSliding;
    private Vector2 lastWallNormal;

    Collider2D playerCollider;

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
        else
            GlobalInput.Instance.OnMouseDown += OnJumpPressed;
    }

    void UnregisterInput()
    {
        if (GlobalInput.Instance == null) return;

        if (playerInput == PlayerInputType.Player1_Space)
            GlobalInput.Instance.OnSpaceDown -= OnJumpPressed;
        else
            GlobalInput.Instance.OnMouseDown -= OnJumpPressed;
    }

    void OnJumpPressed()
    {
        lastJumpPressedTime = Time.time;
    }

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

        if (isGrounded)
            lastGroundedTime = Time.time;

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

        Debug.Log("ObstacleLayer mask = " + obstacleLayer.value);
        DebugState("Check");
    }

    void HandleWallSlide()
    {
        bool previous = isWallSliding;
        if (!isGrounded && touchingObstacle && Mathf.Abs(lastWallNormal.x) > 0.9f)
        {
            isWallSliding = true;

            jumpCount = 0;

            // 自动调整方向为离开墙
            moveDir = -(int)Mathf.Sign(lastWallNormal.x);
        }
        else
        {
            isWallSliding = false;
        }
        if (previous != isWallSliding)
        {
            DebugState("WallSlideChange");
        }
    }

    void HandleMovement()
    {
        rb.gravityScale = normalGravity;

        if (isWallSliding)
        {
            float limitedY = Mathf.Max(rb.velocity.y, -slideSpeed);

            // 关键修改：滑墙时水平速度为0
            rb.velocity = new Vector2(0, limitedY);
        }
        else
        {
            if (isGrounded && touchingObstacle)
            {
                moveDir *= -1;
            }

            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        }
        DebugState("Move");
    }

    void HandleJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool coyoteValid = Time.time - lastGroundedTime <= coyoteTime;

        if (!jumpBuffered)
            return;
        if (isWallSliding)
        {
            DebugState("WallJump");
        }
        if (coyoteValid)
        {
            DebugState("GroundJump");
        }
        if (isWallSliding)
        {
            moveDir *= -1;

            rb.velocity = new Vector2(moveDir * moveSpeed, jumpForce);

            jumpCount = 1;
            lastJumpPressedTime = -10f;
            isWallSliding = false;

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

    void DebugState(string tag)
    {
        Debug.Log(
            $"[{tag}] " +
            $"Grounded:{isGrounded} " +
            $"TouchWall:{touchingObstacle} " +
            $"WallSlide:{isWallSliding} " +
            $"Dir:{moveDir} " +
            $"Vel:{rb.velocity}"
        );
    }
}