using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent meshAgent;

    public void SetDirection(Vector2 moveDirection)
    {
        var movePos = new Vector3(transform.position.x + moveDirection.x, 0, transform.position.z + moveDirection.y);
        meshAgent.SetDestination(movePos);
    }
}