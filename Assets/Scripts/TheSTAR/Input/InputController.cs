using System;
using UnityEngine;

namespace TheSTAR.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private JoystickContainer joystickContainer;
        
        public void Init(Action<Vector2> joystickInputAction) => joystickContainer.Init(joystickInputAction);
    }
}