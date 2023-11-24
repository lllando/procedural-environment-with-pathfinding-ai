using System;
using System.Collections;
using UnityEngine;

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
    [SerializeField] private Transform patrolPoint;

    private float chaseDistance = 10f;
    private float shootingDistance = 2f;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pathfinding = FindFirstObjectByType<Pathfinding>();
    }

    private void Start()
    {
        StartCoroutine(UpdateStateToIdle());
    }

    private void Update()
    {
        CheckState();
    }

    private void CheckState()
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

    #region Individual State Logic
    private void Idle()
    {
    }

    private void Patrol()
    {
        pathfinding.FindPath(transform.position, patrolPoint.position);
        
        if (Vector3.Distance(transform.position, player.position) < chaseDistance)
        {
            currentState = UpdateStateToChase();
        }
    }

    private void Chase()
    {
        pathfinding.FindPath(transform.position, player.position);
        
        // Check if the player is very close
        if (Vector3.Distance(transform.position, player.position) < shootingDistance)
        {
            currentState = UpdateStateToShoot();
        }
    }
    
    private void Shoot()
    {
        
    }
    #endregion

    #region Update States
    private IEnumerator UpdateStateToIdle()
    {
        meshRenderer.material = idleMat;
        currentState = NPCFiniteStateMachine.Idle;
        yield return new WaitForSeconds(5f);
        currentState = UpdateStateToPatrol();
    }
    
    private NPCFiniteStateMachine UpdateStateToPatrol()
    {
        meshRenderer.material = patrolMat;
        return NPCFiniteStateMachine.Patrol;
    }
    
    private NPCFiniteStateMachine UpdateStateToChase()
    {
        meshRenderer.material = chaseMat;
        // navmeshAgent
        return NPCFiniteStateMachine.Chase;
    }
    
    private NPCFiniteStateMachine UpdateStateToShoot()
    {
        meshRenderer.material = shootMat;
        return NPCFiniteStateMachine.Shoot;
    }
    #endregion
}
