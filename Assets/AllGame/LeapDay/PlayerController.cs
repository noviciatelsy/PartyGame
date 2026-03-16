using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    int moveDir = 1;

    [Header("Jump")]
    public float jumpForce = 14f;
    public int maxJump = 2;
    int jumpCount = 0;

    [Header("Wall Slide")]
    public float slideSpeed = 2f;

    Rigidbody2D rb;

    bool isGround;
    bool isWall;
    bool isWallSlide;

    bool wallLeft = false;
    bool wallRight = false;
    bool ground = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    Vector2 contactNormal;
    void FixedUpdate()
    {
        HandleMove();
        contactNormal = Vector2.zero;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    void HandleMove()
    {
        isGround = contactNormal.y > 0.7f;
        isWall = Mathf.Abs(contactNormal.x) > 0.7f;

        isWallSlide = !isGround && isWall;

        if (isWallSlide)
        {
            rb.velocity = new Vector2(
                0,
                Mathf.Max(rb.velocity.y, -slideSpeed)
            );
            return;
        }

        float wallDir = Mathf.Sign(contactNormal.x); // -1 ×óÇ½£¬1 ÓÒÇ½
        if (isGround && isWall && wallDir == moveDir)
            moveDir *= -1;

        rb.velocity = new Vector2(
            moveDir * moveSpeed,
            rb.velocity.y
        );

        if (isGround || isWall)
            jumpCount = 0;
    }

    void Jump()
    {
        if (jumpCount >= maxJump)
            return;

        if (isWallSlide)
        {
            moveDir *= -1;

            rb.velocity = new Vector2(
                moveDir * moveSpeed,
                jumpForce
            );
        }
        else
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                jumpForce
            );
        }

        jumpCount++;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CollectCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CollectCollision(collision);
    }

    void CollectCollision(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            foreach (var c in collision.contacts)
            {
                Vector2 n = c.normal;
                if (n.y > 0.7f) ground = true;
                if (n.x > 0.7f) wallLeft = true;
                if (n.x < -0.7f) wallRight = true;
            }
        }
        Debug.Log(contactNormal);
    }
}