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

    private int health = 100000;
    
    // Exponential distribution variables
    private double lambdaValueForDamageTaken = 0.3; // Smaller lambda value = skew to higher damage values
    
    // Normal distribution variables
    private double meanDamage = 6;
    private double standardDeviationDamage = 3;
    
    // Uniform distribution variables
    private int minDamage = 1;
    private int maxDamage = 10;

    private int pickupCounter = 0;
    
    // Probability damage distributions
    public enum TakeDamageType
    {
        SymmetricalUniform, // Use random number generation
        SymmetricalNormal, // Use normal distribution
        AsymmetricalExponential // Use exponential distribution (lower value weighted)
    }

    public TakeDamageType takeDamageType;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        Debug.Log(PlayerHud.Instance +" player hud");
        
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
            transform.LookAt(shootTowards);
        }
    }

    private void Shoot()
    {
        PlayerBulletController bullet = Instantiate(bulletPrefab, shootPoint.position, transform.rotation).GetComponent<PlayerBulletController>();
        bullet.UpdateBullet(shootTowards);
    }
    
    private void TakeDamage(GameObject from)
    {
        int damage = 1;
        switch (takeDamageType)
        {
            case (TakeDamageType.SymmetricalUniform): 
                damage = CalculateDistributions.UniformDistribution(minDamage, maxDamage);
                break;
            case (TakeDamageType.SymmetricalNormal): 
                damage = CalculateDistributions.NormalDistribution(meanDamage, standardDeviationDamage);
                break;
            case (TakeDamageType.AsymmetricalExponential): 
                damage = CalculateDistributions.ExponentialDistribution(lambdaValueForDamageTaken, 1);
                break;
        }
        health -= damage;
        
        PlayerHud.Instance.UpdateDamageReceived(from, damage); // Update player hud to display damage dealt
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
            TakeDamage(other.GetComponent<NpcBulletController>().ShotBy);
        }
    }
}
