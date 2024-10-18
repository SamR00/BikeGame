using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aiSwichTest : MonoBehaviour
{
    public float moveSpeed = 5f;       // Speed of forward movement
    public float turnSpeed = 2f;       // Speed of turning/steering
    public float health = 100f;        // Enemy health
    public float detectionRange = 5f;  // Increased distance to detect obstacles
    public State currentState = State.FollowPath;
    private Rigidbody rb;
    public Rigidbody target;

    public Transform[] waypoints;      // Array of waypoints for path following
    private int currentWaypointIndex = 0;  // Index of the current waypoint the enemy is moving toward
    public float waypointTolerance = 5f; // Increased distance threshold to consider reaching a waypoint for smoother transitions

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;  // Prevents automatic rotation due to physics
        currentState = State.FollowPath;   
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                //Debug.Log("Waiting...");
                break;

            case State.Attack:
                //Debug.Log("Attacking!");
                break;

            case State.Retreat:
                moveAway();
                //Debug.Log("Run Away!");
                break;

            case State.FollowPath:
                FollowPath();
                //Debug.Log("Following Path...");
                break;
        }
    }

    void FollowPath()
    {
        if (waypoints.Length == 0) return; // Ensure there are waypoints defined

        // Get the current waypoint position.
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetPosition = currentWaypoint.position;

        // Calculate the direction toward the waypoint, keeping movement constrained to the XZ plane.
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Prevent upward or downward movement.

        // Adjust direction if an obstacle is detected using raycasting.
        Vector3 moveDirection = direction;
        if (Physics.Raycast(transform.position, transform.forward, detectionRange))
        {
            // Find a new direction to avoid the obstacle.
            moveDirection = Vector3.Lerp(direction, FindAvoidanceDirection(direction), 0.5f).normalized;
        }

        // Rotate the enemy to face the direction smoothly like a car.
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Apply forward movement in the adjusted direction.
        rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);

        // Check if the enemy is close enough to the waypoint to consider it reached.
        if (Vector3.Distance(transform.position, targetPosition) < waypointTolerance)
        {
            // Move to the next waypoint, loop back if at the last one.
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    void moveAway()
    {
        // Calculate the direction to move away from the target on the XZ plane.
        Vector3 direction = (transform.position - target.position).normalized;
        direction.y = 0;  // Prevent movement on the Y-axis.
        direction.Normalize();  // Normalize the direction again to keep consistent speed.

        // Rotate smoothly like a car.
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Apply movement away from the target.
        rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.deltaTime);
    }

    Vector3 FindAvoidanceDirection(Vector3 currentDirection)
    {
        Vector3 right = transform.right;
        Vector3 left = -transform.right;

        // Check if moving right is clear.
        if (!Physics.Raycast(transform.position, right, detectionRange))
        {
            return (right + currentDirection).normalized; // Move slightly to the right while still heading toward the waypoint.
        }
        // Check if moving left is clear.
        else if (!Physics.Raycast(transform.position, left, detectionRange))
        {
            return (left + currentDirection).normalized; // Move slightly to the left while still heading toward the waypoint.
        }

        // If both sides are blocked, move in the opposite direction (slows down).
        return -currentDirection;
    }
}

[System.Serializable]
public enum State { Idle, Attack, Retreat, FollowPath }
