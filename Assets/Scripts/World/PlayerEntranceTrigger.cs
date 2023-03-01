using System;
using UnityEngine;

namespace World
{
    public class PlayerEntranceTrigger : MonoBehaviour
    {
        private Action _onTriggerEnter;
        private Action _onTriggerExit;

        public void Init(Action onEnter, Action onExit)
        {
            _onTriggerEnter = onEnter;
            _onTriggerExit = onExit;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _onTriggerEnter?.Invoke();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) _onTriggerExit?.Invoke();
        }
    }
}