using System;
using System.Collections;
using UnityEngine;

public class NpcBehaviour : MonoBehaviour
{
    enum FiniteStateMachine
    {
        Idle,
        Patrol,
        Chase,
        Shoot
    }

    private FiniteStateMachine currentState;

    private void Awake()
    {
        throw new NotImplementedException();
    }

    private void Update()
    {
        CheckState();
    }

    private void CheckState()
    {
        switch (currentState)
        {
            case (FiniteStateMachine.Idle):
                Idle();
                break;
            case (FiniteStateMachine.Patrol):
                Patrol();
                break;
            case (FiniteStateMachine.Chase):
                Chase();
                break;
            case (FiniteStateMachine.Shoot):
                Shoot();
                break;
        }
    }

    private void Idle()
    {
        throw new NotImplementedException();
    }

    private void Patrol()
    {
        throw new NotImplementedException();
    }

    private void Chase()
    {
        throw new NotImplementedException();
    }
    
    private void Shoot()
    {
        throw new NotImplementedException();
    }
}
