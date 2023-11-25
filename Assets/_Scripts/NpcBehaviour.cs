using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcBehaviour : MonoBehaviour
{
    enum NPCFiniteStateMachine
    {
        Idle,
        Patrol,
        Chase,
        Shoot
    }

    [SerializeField] private Material idleMat;
    [SerializeField] private Material patrolMat;
    [SerializeField] private Material chaseMat;
    [SerializeField] private Material shootMat;

    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI healthText;

    private MeshRenderer meshRenderer;
    
    private NPCFiniteStateMachine currentState;

    private Pathfinding pathfinding;

    [SerializeField] private Transform player;
    private Transform lookAtPlayer;
    private Transform destination;

    private float chaseDistance = 25f;
    private float shootingDistance = 6f;
    private float patrolPointDistance = 2f;

    private CheckNavMesh checkNavMesh;
    private List<GameObject> pickupObjects;

    private float turnSpeed = 10f;
    private float shootCooldown;
    private float fireRate = 2f;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;

    private float health = 100f;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pathfinding = FindFirstObjectByType<Pathfinding>();
        checkNavMesh = FindFirstObjectByType<CheckNavMesh>();
        
        

        lookAtPlayer = player.GetChild(0);
    }

    private void Start()
    {
        pickupObjects = checkNavMesh.spawnedPickupObjects;
        UpdateHealthVisual();
        // StartCoroutine(EnteredIdleState());
        currentState = UpdateState(NPCFiniteStateMachine.Patrol);
    }

    private void Update()
    {
        RunStateLogic();
    }

    private void RunStateLogic()
    {
        switch (currentState)
        {
            case (NPCFiniteStateMachine.Idle):
                Idle();
                break;
            case (NPCFiniteStateMachine.Patrol):
                Patrol();
                break;
            case (NPCFiniteStateMachine.Chase):
                Chase();
                break;
            case (NPCFiniteStateMachine.Shoot):
                Shoot();
                break;
        }
    }
    
    #region Update States
    private IEnumerator EnteredIdleState()
    {
        meshRenderer.material = idleMat;
        yield return new WaitForSeconds(5f);
        currentState = UpdateState(NPCFiniteStateMachine.Patrol);
    }

    private NPCFiniteStateMachine UpdateState(NPCFiniteStateMachine newState)
    {
        switch (newState)
        {
            case (NPCFiniteStateMachine.Idle):
                stateText.text = "Idle";
                stateText.color = idleMat.color;
                StartCoroutine(EnteredIdleState());
                break;
            case (NPCFiniteStateMachine.Patrol):
                stateText.text = "Patrol";
                stateText.color = patrolMat.color;
                meshRenderer.material = patrolMat;
                destination = GetRandomPickupObject();
                break;
            case (NPCFiniteStateMachine.Chase):
                stateText.text = "Chase";
                stateText.color = chaseMat.color;
                meshRenderer.material = chaseMat;
                break;
            case (NPCFiniteStateMachine.Shoot):
                stateText.text = "Shoot";
                stateText.color = shootMat.color;
                meshRenderer.material = shootMat;
                break;
        }

        return newState;
    }
    #endregion

    #region Individual State Logic
    private void Idle()
    {
        
    }

    private void Patrol()
    {
        // AimAt();

        pathfinding.FindPath(transform.position, destination.position);

        if (Vector3.Distance(transform.position, destination.position) < patrolPointDistance)
        {
            // destination = GetRandomPickupObject();
            currentState = UpdateState(NPCFiniteStateMachine.Idle);
        }
        if (Vector3.Distance(transform.position, player.position) < chaseDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Chase);
        }
    }

    private void Chase()
    {
        AimAt(player.position);

        // Check whether the player is in shoot range
        if (Vector3.Distance(transform.position, player.position) > chaseDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Patrol);
        }
        
        pathfinding.FindPath(transform.position, player.position);
        
        // Check whether the player is in shoot range
        if (Vector3.Distance(transform.position, player.position) < shootingDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Shoot);
        }
    }
    
    private void Shoot()
    {
        pathfinding.ClearPath();

        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }
        
        AimAt(player.position);

        if (CanShoot())
        {
            shootCooldown = 1f / fireRate;

            NpcBulletController bullet = Instantiate(bulletPrefab, shootPoint.position, transform.rotation).GetComponent<NpcBulletController>();
            bullet.UpdateBullet(lookAtPlayer);
        }
        // Check whether the player is in shoot range
        if (Vector3.Distance(transform.position, player.position) > shootingDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Chase);
        }
    }
    #endregion

    private Transform GetRandomPickupObject()
    {
        int randomPickupObject = Random.Range(0, pickupObjects.Count);
        return pickupObjects[randomPickupObject].transform;
    }

    private void AimAt(Vector3 aimAt)
    {
        Vector3 direction = (aimAt - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    private bool CanShoot()
    {
        return shootCooldown <= 0f;
    }

    private void TakeDamage(int damage)
    {
        health -= damage;

        UpdateHealthVisual();
        if (health <= 0)
        {
            Die();
        }

    }

    private void UpdateHealthVisual()
    {
        healthText.text = health.ToString();
        healthText.color = Color.Lerp(Color.red, Color.green, health / 100f);
    }

    private void Die()
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(25);
        }
    }
}
