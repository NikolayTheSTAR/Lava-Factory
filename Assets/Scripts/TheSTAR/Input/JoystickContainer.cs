using System;
using TheSTAR.Utility;
using TheSTAR.Utility.Pointer;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheSTAR.Input
{
    public class JoystickContainer : MonoBehaviour
    {
        [SerializeField] private Pointer pointer;
        [SerializeField] private GameObject stickObject;
        [SerializeField] private float limitDistance = 50;

        private Action<Vector2> _joystickInputAction;
        private bool _isDown = false;

        public void Init(Action<Vector2> joystickInoutAction)
        {
            pointer.InitPointer(
                (eventData) => OnJoystickDown(), 
                (eventData) => OnJoystickDrag(),
                (eventData) => OnJoystickUp());

            _joystickInputAction = joystickInoutAction;
        }

        private void Update()
        {
            JoystickInput();
        }

        private void OnJoystickDown()
        {
            _isDown = true;
            UpdateStickPosByMouse();
        }
    
        private void OnJoystickDrag()
        {
            _isDown = true;
            UpdateStickPosByMouse();
        }
    
        private void OnJoystickUp()
        {
            _isDown = false;
            stickObject.transform.localPosition = Vector2.zero;
        }

        private void UpdateStickPosByMouse()
        {
            stickObject.transform.position = UnityEngine.Input.mousePosition;
            stickObject.transform.localPosition = MathUtility.LimitForCircle(stickObject.transform.localPosition, limitDistance);
        }

        private void JoystickInput()
        {
            if (_isDown) _joystickInputAction?.Invoke(stickObject.transform.localPosition / limitDistance);
            else _joystickInputAction?.Invoke(Vector2.zero);
        }
    }
}