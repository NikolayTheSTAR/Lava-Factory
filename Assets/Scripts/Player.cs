using System;
using System.Collections;
using System.Collections.Generic;
using TheSTAR.Input;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Player : MonoBehaviour, ICameraFocusable, IJoystickControlled
{
    [SerializeField] private NavMeshAgent meshAgent;

    public void JoystickInput(Vector2 input)
    {
        Vector3 finalMoveDirection;

        if (input.x != 0 || input.y != 0)
        {
            var tempMoveDirection = new Vector3(input.x, 0, input.y);
            finalMoveDirection = transform.position + tempMoveDirection;
        }
        else finalMoveDirection = transform.position;

        meshAgent.SetDestination(finalMoveDirection);
    }
}