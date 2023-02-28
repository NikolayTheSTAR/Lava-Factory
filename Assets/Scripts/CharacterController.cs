using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent meshAgent;
    
    /*
    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        SetDestination(new Vector2(x, y));
    }
    */

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