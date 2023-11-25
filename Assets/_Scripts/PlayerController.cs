using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 movement;

    private Camera mainCamera;
    
    public float downwardForce = 10f; // Additional gravity or downward force

    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootTowards;

    private int health = 100;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        
        PlayerHud.Instance.UpdateHealth(health);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            FaceMousePosition();
            Shoot();
        }
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

    private void FaceMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            shootTowards.position = raycastHit.point;
        }
    }

    private void Shoot()
    {
        PlayerBulletController bullet = Instantiate(bulletPrefab, shootPoint.position, transform.rotation).GetComponent<PlayerBulletController>();
        bullet.UpdateBullet(shootTowards);
    }
    
    private void TakeDamage(int damage)
    {
        health -= damage;

        PlayerHud.Instance.UpdateHealth(health);
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Destroy(this.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPCBullet"))
        {
            TakeDamage(10);
        }
    }
}
