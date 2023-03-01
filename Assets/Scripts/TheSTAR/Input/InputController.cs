using System;
using UnityEngine;

namespace TheSTAR.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private JoystickContainer joystickContainer;

        public void Init(IJoystickControlled j)
        {
            joystickContainer.Init(j != null ? j.JoystickInput : null);
        }
    }

    public interface IJoystickControlled
    {
        void JoystickInput(Vector2 input);
    }
}