using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    private Rigidbody2D rb;
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float jumpPeakSpeedMultiplier = 1.5f;
    [SerializeField] private float speedTransitionSmoothness = 5f;
    private float xAxis;
    private float currentSpeed;
    private int facingDirection = 1;

    [Header("Ground Check Settings")]
    [SerializeField] private float jumpForce = 45;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatisGround;
    
    [Header("Jump Settings")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float jumpPeakThreshold = 0.5f;
    
    // Ability state variables
    private bool isDashing = false;
    // private bool canDoubleJump = true;
    // private bool hasDoubleJumped = false;
    private float dashTimeLeft = 0f;
    public float dashDuration = 0.5f;
    public float dashSpeed = 3f;
    [SerializeField] private int maxJumps = 2;
    private int jumpsLeft;

    
    public static PlayerMovement Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = walkSpeed;
        facingDirection = (int)transform.localScale.x;
    }

    void Update()
    {
        getInputs();

        if (Grounded()) jumpsLeft = maxJumps;
        jump();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            move();
        }
        flip();
        applyJumpPhysics();
        clampFallSpeed();
        HandleDash();
    }

    void getInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }

    void flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-5, transform.localScale.y);
            facingDirection = -1;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(5, transform.localScale.y);
            facingDirection = 1;
        }
    }

    private void move()
    {
        
        
        float targetSpeed = walkSpeed;
        
        if (!Grounded() && Mathf.Abs(rb.velocity.y) < jumpPeakThreshold)
        {
            targetSpeed = walkSpeed * jumpPeakSpeedMultiplier;
        }
        
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedTransitionSmoothness * Time.fixedDeltaTime);
        rb.velocity = new Vector2(currentSpeed * xAxis, rb.velocity.y);
    }

    public bool Grounded()
    {
        return Physics2D.BoxCast(groundCheckPoint.position, 
                               new Vector2(groundCheckX, groundCheckY), 
                               0f, Vector2.down, 0f, whatisGround);
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsLeft--;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    void TryDash()
    {
        if (!isDashing && (Grounded() || Mathf.Abs(rb.velocity.x) > 0.1f))
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
        }
    }
    
    void HandleDash()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                rb.velocity = new Vector2(dashSpeed * facingDirection, 0);
                dashTimeLeft -= Time.fixedDeltaTime;
            }
            else
            {
                isDashing = false;
            }
        }
    }
    
    void applyJumpPhysics()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    void clampFallSpeed()
    {
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
    }
}