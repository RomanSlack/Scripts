using UnityEngine;

public class PlayerController_V3 : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    public LayerMask solidObjectsLayer;

    private Vector2 movement;
    private Vector2 lastNonZeroMovement;
    private Vector2 smoothVelocity;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.gravityScale = 0f; // Ensure gravity is not affecting the player
        lastNonZeroMovement = Vector2.down; // Default facing down
    }

    private void Update()
    {
        // Get input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Prioritize horizontal movement
        if (movement.x != 0) movement.y = 0;

        // Normalize movement vector
        if (movement != Vector2.zero)
        {
            movement.Normalize();
        }

        // Update last non-zero movement for idle facing direction
        if (movement != Vector2.zero)
        {
            lastNonZeroMovement = movement;
        }

        // Update animator parameters
        animator.SetFloat("moveX", lastNonZeroMovement.x);
        animator.SetFloat("moveY", lastNonZeroMovement.y);

        // Debug logs
        Debug.Log("This is input.x: " + movement.x);
        Debug.Log("This is input.y: " + movement.y);
    }

    private void FixedUpdate()
    {
        Vector2 targetPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        if (IsWalkable(targetPosition))
        {
            // Calculate target velocity
            Vector2 targetVelocity = movement * moveSpeed;

            // Smoothly interpolate between current velocity and target velocity
            smoothVelocity = Vector2.MoveTowards(smoothVelocity, targetVelocity, GetAcceleration() * Time.fixedDeltaTime);

            // Apply movement
            rb.MovePosition(rb.position + smoothVelocity * Time.fixedDeltaTime);

            isMoving = smoothVelocity.magnitude > 0.1f;
        }
        else
        {
            // If the target position is not walkable, stop movement
            smoothVelocity = Vector2.zero;
            isMoving = false;
        }

        // Update the animator
        animator.SetBool("isMoving", isMoving);
    }

    private float GetAcceleration()
    {
        // Use acceleration when there's input, otherwise use deceleration
        return movement.sqrMagnitude > 0 ? acceleration : deceleration;
    }

    private bool IsWalkable(Vector2 targetPos)
    {
        return Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) == null;
    }
}
