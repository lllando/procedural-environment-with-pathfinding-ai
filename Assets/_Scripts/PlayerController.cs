using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 movement;
    
    public float downwardForce = 10f; // Additional gravity or downward force

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input from WASD keys
        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        movement.Normalize(); // Normalize to ensure consistent speed in all directions
    }

    void FixedUpdate()
    {
        // Set the velocity directly
        rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.z * moveSpeed);
        
        // Apply additional downward force
        rb.AddForce(Vector3.down * downwardForce, ForceMode.Acceleration);
    }
}
