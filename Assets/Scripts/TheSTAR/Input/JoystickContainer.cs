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
            
        private void Start()
        {
            Init();   
        }

        private void Init()
        {
            pointer.InitPointer(
                (eventData) => OnJoystickDown(), 
                (eventData) => OnJoystickDrag(),
                (eventData) => OnJoystickUp());
        }

        private void OnJoystickDown()
        {
            UpdateStickPosByMouse();
        }
    
        private void OnJoystickDrag()
        {
            UpdateStickPosByMouse();
        }
    
        private void OnJoystickUp()
        {
            stickObject.transform.localPosition = Vector2.zero;
        }

        private void UpdateStickPosByMouse()
        {
            stickObject.transform.position = UnityEngine.Input.mousePosition;
            stickObject.transform.localPosition = MathUtility.LimitForCircle(stickObject.transform.localPosition, limitDistance);
        }
    }
}