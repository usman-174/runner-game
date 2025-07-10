using UnityEngine;

/*
    This script rotates a wall/stick obstacle continuously
    Player can pass through when the wall is rotated to allow passage
*/

public class Rotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // Degrees per second
    public Vector3 rotationAxis = Vector3.up; // Which axis to rotate around (Y-axis by default)
    
    [Header("Optional Settings")]
    public bool randomStartRotation = true; // Start at random rotation
    public bool reverseDirection = false; // Rotate in opposite direction
    
    void Start()
    {
        // Optionally start at a random rotation
        if (randomStartRotation)
        {
            transform.rotation = Quaternion.Euler(
                Random.Range(0f, 360f) * rotationAxis.x,
                Random.Range(0f, 360f) * rotationAxis.y,
                Random.Range(0f, 360f) * rotationAxis.z
            );
        }
    }
    
    void Update()
    {
        // Rotate the wall continuously
        float rotationAmount = rotationSpeed * Time.deltaTime;
        
        if (reverseDirection)
            rotationAmount = -rotationAmount;
            
        transform.Rotate(rotationAxis * rotationAmount);
    }
}