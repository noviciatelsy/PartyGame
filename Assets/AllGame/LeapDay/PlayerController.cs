using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerInputType
    {
        Player1_Space,
        Player2_Mouse
    }
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

    [Header("Wall")]
    public float slideSpeed = 3f;
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public float wallCheckDistance = 0.7f;
    public float groundCheckDistance = 1.1f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    private Rigidbody2D rb;

    private bool isGrounded;
    private bool touchingWall;
    private bool isWallSliding;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        RegisterInput();
    }

    void OnDestroy()
    {
        UnregisterInput();
    }

    // =========================
    // INPUT
    // =========================

    void RegisterInput()
    {
        if (GlobalInput.Instance == null) return;

        if (playerInput == PlayerInputType.Player1_Space)
        {
            GlobalInput.Instance.OnSpaceDown += OnJumpPressed;
        }
        else if (playerInput == PlayerInputType.Player2_Mouse)
        {
            GlobalInput.Instance.OnMouseDown += OnJumpPressed;
        }
    }

    void UnregisterInput()
    {
        if (GlobalInput.Instance == null) return;

        if (playerInput == PlayerInputType.Player1_Space)
        {
            GlobalInput.Instance.OnSpaceDown -= OnJumpPressed;
        }
        else if (playerInput == PlayerInputType.Player2_Mouse)
        {
            GlobalInput.Instance.OnMouseDown -= OnJumpPressed;
        }
    }

    void OnJumpPressed()
    {
        lastJumpPressedTime = Time.time;
    }

    //void Update()
    //{
    //    // 记录跳跃输入
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        lastJumpPressedTime = Time.time;
    //    }
    //}

    void FixedUpdate()
    {
        CheckEnvironment();
        HandleWallSlide();
        HandleJump();
        HandleMovement();
    }

    void CheckEnvironment()
    {
        //isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float height = 0.7f;

        Vector2 originTop = new Vector2(transform.position.x, transform.position.y + height / 2);
        Vector2 originMid = transform.position;
        Vector2 originBot = new Vector2(transform.position.x, transform.position.y - height / 2);

        Vector2 dir = new Vector2(moveDir, 0);

        RaycastHit2D hit1 = Physics2D.Raycast(originTop, dir, wallCheckDistance, wallLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(originMid, dir, wallCheckDistance, wallLayer);
        RaycastHit2D hit3 = Physics2D.Raycast(originBot, dir, wallCheckDistance, wallLayer);

        touchingWall = hit1.collider || hit2.collider || hit3.collider;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            jumpCount = 0;
        }
    }

    void HandleWallSlide()
    {
        if (!isGrounded && touchingWall)
        {
            isWallSliding = true;

            //rb.gravityScale = 4;

            //// 限制最大下落速度
            //if (rb.velocity.y < -slideSpeed)
            //{
            //    rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
            //}
        }
        else
        {
            isWallSliding = false;
        }
    }

    void HandleJump()
    {
        bool jumpBuffered = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool coyoteValid = Time.time - lastGroundedTime <= coyoteTime;

        if (!jumpBuffered) return;

        // 墙跳
        if (isWallSliding)
        {
            moveDir *= -1;

            rb.velocity = new Vector2(moveDir * moveSpeed, jumpForce);

            jumpCount = 1;
            lastJumpPressedTime = -10f;
            isWallSliding = false;

            return;
        }

        // 地面跳 / Coyote
        if (coyoteValid)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            jumpCount = 1;
            lastJumpPressedTime = -10f;

            return;
        }

        // 二段跳
        if (jumpCount < maxJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            jumpCount++;
            lastJumpPressedTime = -10f;
        }
    }

    private float wallSlideDecay = 80f;
    void HandleMovement()
    {
        // 地面撞墙反向
        if (isGrounded && touchingWall)
        {
            moveDir *= -1;
        }

        if (isWallSliding)
        {
            //rb.gravityScale = 0;
            //rb.velocity = new Vector2(0, Mathf.Max(-slideSpeed, rb.velocity.y) );
            rb.gravityScale = 4;

            float newY = Mathf.Lerp(
                rb.velocity.y,
                -slideSpeed,
                wallSlideDecay * Time.fixedDeltaTime
            );

            rb.velocity = new Vector2(0, newY);
        }
        else
        {
            rb.gravityScale = 4;
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        }
    }
}