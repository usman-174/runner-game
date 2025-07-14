using UnityEngine;

/*
    This script moves an obstacle left and right between two boundaries (using collision).
    Attach colliders with "LeftBoundary" and "RightBoundary" tags to both sides.
*/

public class ObstacleMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Speed of obstacle

    private Vector3 moveDirection = Vector3.right; // Start moving right

    void Update()
    {
        // Move the obstacle continuously in current direction
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("leftboundary") || other.CompareTag("rightboundary"))
        {
            // Invert the direction
            moveDirection = -moveDirection;
        }
    }
}
