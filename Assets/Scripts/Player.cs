using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Player : MonoBehaviour, ICameraFocusable
{
    [SerializeField] private NavMeshAgent meshAgent;

    public void SetDestination(Vector2 direction)
    {
        Vector3 finalMoveDirection;

        if (direction.x != 0 || direction.y != 0)
        {
            var tempMoveDirection = new Vector3(direction.x, 0, direction.y);
            finalMoveDirection = transform.position + tempMoveDirection;
        }
        else finalMoveDirection = transform.position;

        meshAgent.SetDestination(finalMoveDirection);
    }
}