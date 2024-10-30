using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 5f;       // Speed of forward movement
    public float turnSpeed = 100f;     // Speed of turning/steering
    public float health = 100f;        // Enemy health

    private float originalMoveSpeed; // Store the original movement speed
    private bool isSlowed = false; // Track if the enemy is slowed

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        originalMoveSpeed = moveSpeed; 
    }



    private void OnCollisionEnter(Collision collision)
    {
        // You can handle what happens when the bullet hits something here (if needed)
        // For example: Only destroy the bullet if it hits certain objects
        if (collision.collider.CompareTag("bullet"))
        {
            TakeDamage(20f);  // Example damage value
            Destroy(collision.gameObject);
        }

    }
    // Function to apply damage to the enemy
    private void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        Debug.Log("Enemy took damage, health is now: " + health);

        // If health is less than or equal to 0, destroy the enemy
        if (health <= 0f)
        {
            Destroy(gameObject);
            Debug.Log("Enemy destroyed");
        }
    }
    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Get the input for moving forward/backward and turning left/right
        float moveInput = 0f;
        float turnInput = 0f;

        // Move forward (I) or backward (K)
        if (Input.GetKey(KeyCode.I))
        {
            moveInput = 1f;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            moveInput = -1f;
        }

        // Turn left (J) or right (L)
        if (Input.GetKey(KeyCode.J))
        {
            turnInput = -1f;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            turnInput = 1f;
        }

        // Handle turning the object smoothly
        if (turnInput != 0f)
        {
            float turnAmount = turnInput * turnSpeed * Time.deltaTime;
            transform.Rotate(0, turnAmount, 0);  // Rotate the object around Y-axis
        }

        // Handle forward/backward movement
        if (moveInput != 0f)
        {
            Vector3 forwardMovement = transform.forward * moveInput * moveSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + forwardMovement);  // Move the object forward in its facing direction
        }
    }

    public void ApplySlow(float duration, float slowFactor)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            moveSpeed *= slowFactor; // Reduce the move speed

            // Start a coroutine to reset the speed after the duration
            StartCoroutine(ResetSpeedAfterDelay(duration));
        }
    }

    private System.Collections.IEnumerator ResetSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = originalMoveSpeed; // Restore the original speed
        isSlowed = false; // Reset the slowed state
    }
}
