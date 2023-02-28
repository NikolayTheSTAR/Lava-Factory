using System;
using UnityEngine;

namespace TheSTAR.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private float forceX;
        [SerializeField] private float forceY;
        [SerializeField] private JoystickContainer joystickContainer;
        [SerializeField] private CharacterController character;

        private void Start()
        {
            joystickContainer.Init(OnJoystickEvent);
        }

        private void OnJoystickEvent(Vector2 direction)
        {
            forceX = direction.x;
            forceY = direction.y;
            
            character.SetDestination(direction);
        }
    }
}