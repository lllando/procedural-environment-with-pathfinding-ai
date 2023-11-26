using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcBehaviour : MonoBehaviour
{
    public enum EnemyType
    {
        Scout, // Uses A* pathfinding
        Brute // Uses BFS pathfinding
    }

    public EnemyType enemyType = EnemyType.Scout;
    
    enum NPCFiniteStateMachine
    {
        Idle,
        Patrol,
        Chase,
        Shoot
    }

    public enum PathfindingType
    {
        AStar,
        BFS
    }

    public PathfindingType pathfindingType = PathfindingType.AStar;

    [SerializeField] private Material idleMat;
    [SerializeField] private Material patrolMat;
    [SerializeField] private Material chaseMat;
    [SerializeField] private Material shootMat;

    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI critText;
    [SerializeField] private TextMeshProUGUI pickupCounterText;
    
    private MeshRenderer meshRenderer;
    
    private NPCFiniteStateMachine currentState;

    private Pathfinding pathfinding;

    private Transform player;
    private Transform lookAtPlayer;
    private Transform destination;

    private float chaseDistance = 12f;
    private float shootingDistance = 6f;
    private float patrolPointDistance = 2f;

    private CheckNavMesh checkNavMesh;
    // private List<GameObject> pickupObjects;

    private float turnSpeed = 10f;
    private float shootCooldown;
    private float fireRate = 2f;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;

    private float health = 100f;
    
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

    [SerializeField] private LayerMask terrainLayerMask;
    
    private void Awake()
    {
        /*
        meshRenderer = GetComponent<MeshRenderer>();
        pathfinding = GetComponent<Pathfinding>();
        checkNavMesh = FindFirstObjectByType<CheckNavMesh>();

        player = FindFirstObjectByType<PlayerController>().transform;

        lookAtPlayer = player.GetChild(0);

        int randomEnemyType = Random.Range(0, 2);
        enemyType = randomEnemyType == 0 ? EnemyType.Scout : EnemyType.Brute;
        
        InitializeValuesForEnemyType();
        */
    }

    private void Start()
    {
        /*
        // pickupObjects = checkNavMesh.spawnedPickupObjects;
        
        UpdateHealthDisplay();
        pickupCounterText.text = pickupCounter.ToString();
        
        // StartCoroutine(EnteredIdleState());
        currentState = UpdateState(NPCFiniteStateMachine.Patrol);
        */
    }

    public void InitializeNpc()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pathfinding = GetComponent<Pathfinding>();
        checkNavMesh = FindFirstObjectByType<CheckNavMesh>();

        player = FindFirstObjectByType<PlayerController>().transform;

        lookAtPlayer = player.GetChild(0);

        int randomEnemyType = Random.Range(0, 2);
        enemyType = randomEnemyType == 0 ? EnemyType.Scout : EnemyType.Brute;
        
        InitializeValuesForEnemyType();
        
        // pickupObjects = checkNavMesh.spawnedPickupObjects;
        
        UpdateHealthDisplay();
        pickupCounterText.text = pickupCounter.ToString();
        
        // StartCoroutine(EnteredIdleState());
        currentState = UpdateState(NPCFiniteStateMachine.Patrol);
    }

    private void Update()
    {
        FixSlopeMovement();
        RunStateLogic();
    }

    private void InitializeValuesForEnemyType()
    {
        int randomDamageType = Random.Range(0, 3);
        
        if (randomDamageType == 0)
            takeDamageType = TakeDamageType.SymmetricalUniform;
        else if (randomDamageType == 1)
            takeDamageType = TakeDamageType.SymmetricalNormal;
        else if (randomDamageType == 2)
            takeDamageType = TakeDamageType.AsymmetricalExponential;
        
        switch (enemyType)
        {
            case EnemyType.Scout:
                idText.text = "NPC_Scout";
                health = 100;
                pathfindingType = PathfindingType.AStar;
                pathfinding.moveSpeed = 0.01f;
                break;
            case EnemyType.Brute:
                idText.text = "NPC_Brute";
                health = 250;
                chaseDistance /= 2;
                pathfindingType = PathfindingType.BFS;
                pathfinding.moveSpeed = 0.005f;
                meanDamage *= 2;
                minDamage *= 2;
                maxDamage *= 2;
                fireRate *= 2;
                lambdaValueForDamageTaken = 0.2;
                break;
        }
    }

    private void FixSlopeMovement()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, terrainLayerMask))
        {
            // Set the NPC to be on the ground with a slight y offset so it is does not get stuck
            transform.position = new Vector3(transform.position.x, hit.point.y + 1f, transform.position.z);
            // objectTransform.up = hit.normal; // Used to adjust the rotation of the NPC based on the terrain it is on 
        }
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
                Debug.Log("Entered patrol state");
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
        if (destination == null)
        {
            destination = GetRandomPickupObject();
        }
        
        AimAt(destination.position);
        
        bool isValidPath = FindPathBasedOnPathfindingType(transform.position, destination.position);
        Debug.Log("Valid path? " + isValidPath);
        
        if (!isValidPath)
            destination = GetRandomPickupObject();

        if (Vector3.Distance(transform.position, destination.position) < patrolPointDistance)
        {
            pickupCounter++;
            pickupCounterText.text = pickupCounter.ToString();

            checkNavMesh.spawnedPickupObjects.Remove(destination.gameObject); // Remove collected pickup object from list of pickup objects
            Destroy(destination.gameObject);
            
            currentState = UpdateState(NPCFiniteStateMachine.Idle);
        }
        if (Vector3.Distance(transform.position, player.position) < chaseDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Chase);
        }
    }

    private void Chase()
    {
        if (player == null)
        {
            UpdateState(NPCFiniteStateMachine.Patrol);
            return;
        }
        
        AimAt(player.position);

        // Check whether the player is in shoot range
        if (Vector3.Distance(transform.position, player.position) > chaseDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Patrol);
        }
        bool isValidChasePath = FindPathBasedOnPathfindingType(transform.position, player.position);
        Debug.Log("Valid chase path? " + isValidChasePath);

        
        // Check whether the player is in shoot range
        if (Vector3.Distance(transform.position, player.position) < shootingDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Shoot);
        }
    }
    
    private void Shoot()
    {
        if (player == null)
        {
            UpdateState(NPCFiniteStateMachine.Patrol);
            return;
        }
        
        pathfinding.ClearPath();

        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }
        
        AimAt(player.position); // Aim at the player

        if (CanShoot())
        {
            shootCooldown = 1f / fireRate;

            NpcBulletController bullet = Instantiate(bulletPrefab, shootPoint.position, transform.rotation).GetComponent<NpcBulletController>();
            bullet.SetShooter(this.gameObject);
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
        Debug.Log("NAVMESH COUNT: " + checkNavMesh.spawnedPickupObjects.Count);
        int randomPickupObject = Random.Range(0, checkNavMesh.spawnedPickupObjects.Count);
        Debug.Log($"Blobs remaining: {checkNavMesh.spawnedPickupObjects.Count}. Getting blob at index: {randomPickupObject} for {gameObject.name}");
        return checkNavMesh.spawnedPickupObjects[randomPickupObject].transform;
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
    
    private void TakeDamage()
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
        
        PlayerHud.Instance.UpdateDamageDealt(this.gameObject, damage); // Update player hud to display damage dealt

        if (damage >= 10)
        {
            StartCoroutine(DisplayCritText());
        }
        
        Debug.Log($"Took {damage} damage. Health is now at {health}");

        UpdateHealthDisplay();
    
        if (health <= 0)
        {
            Die();
        }
    }

    private bool FindPathBasedOnPathfindingType(Vector3 startPos, Vector3 targetPos)
    {
        switch (pathfindingType)
        {
            case PathfindingType.AStar:
                return pathfinding.FindPath(startPos, targetPos);
            case PathfindingType.BFS:
                return pathfinding.FindPathUsingBFS(startPos, targetPos);
        }

        return false;
    }

    private void UpdateHealthDisplay()
    {
        healthText.text = health.ToString();
        healthText.color = Color.Lerp(Color.red, Color.green, health / 100f);
    }

    private IEnumerator DisplayCritText()
    {
        critText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        critText.gameObject.SetActive(false);
    }

    private void Die()
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage();
        }
    }
}
