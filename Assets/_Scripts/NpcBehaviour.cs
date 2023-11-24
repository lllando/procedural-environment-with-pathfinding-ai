using System;
using System.Collections;
using System.Collections.Generic;
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

    private MeshRenderer meshRenderer;
    
    private NPCFiniteStateMachine currentState;

    private Pathfinding pathfinding;

    [SerializeField] private Transform player;
    [SerializeField] private Transform destination;

    private float chaseDistance = 10f;
    private float shootingDistance = 2f;

    private CheckNavMesh checkNavMesh;
    private List<GameObject> pickupObjects;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pathfinding = FindFirstObjectByType<Pathfinding>();
        checkNavMesh = FindFirstObjectByType<CheckNavMesh>();
    }

    private void Start()
    {
        pickupObjects = checkNavMesh.spawnedPickupObjects;   
        StartCoroutine(UpdateStateToIdle());
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
                // Chase();
                break;
            case (NPCFiniteStateMachine.Shoot):
                Shoot();
                break;
        }
    }
    
    #region Update States
    private IEnumerator UpdateStateToIdle()
    {
        meshRenderer.material = idleMat;
        currentState = NPCFiniteStateMachine.Idle;
        yield return new WaitForSeconds(5f);
        currentState = UpdateState(NPCFiniteStateMachine.Patrol);
    }

    private NPCFiniteStateMachine UpdateState(NPCFiniteStateMachine newState)
    {
        switch (newState)
        {
            case (NPCFiniteStateMachine.Idle):
                meshRenderer.material = idleMat;
                break;
            case (NPCFiniteStateMachine.Patrol):
                meshRenderer.material = patrolMat;
                destination = GetRandomPickupObject();
                break;
            case (NPCFiniteStateMachine.Chase):
                meshRenderer.material = chaseMat;
                break;
            case (NPCFiniteStateMachine.Shoot):
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
        pathfinding.FindPath(transform.position, destination.position);
        
        if (Vector3.Distance(transform.position, player.position) < chaseDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Chase);
        }
    }

    private void Chase()
    {
        pathfinding.FindPath(transform.position, player.position);
        
        // Check if the player is very close
        if (Vector3.Distance(transform.position, player.position) < shootingDistance)
        {
            currentState = UpdateState(NPCFiniteStateMachine.Shoot);
        }
    }
    
    private void Shoot()
    {
        
    }
    #endregion

    private Transform GetRandomPickupObject()
    {
        int randomPickupObject = Random.Range(0, pickupObjects.Count);
        return pickupObjects[randomPickupObject].transform;
    }
    

}
