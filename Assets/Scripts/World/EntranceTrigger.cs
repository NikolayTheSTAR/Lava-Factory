using System;
using UnityEngine;

namespace World
{
    public class EntranceTrigger : MonoBehaviour
    {
        private Action<Collider> _onTriggerEnter;
        private Action<Collider> _onTriggerExit;

        public void Init(Action<Collider> onEnter, Action<Collider> onExit)
        {
            _onTriggerEnter = onEnter;
            _onTriggerExit = onExit;
        }

        private void OnTriggerEnter(Collider other)
        {
            _onTriggerEnter?.Invoke(other);
        }
        
        private void OnTriggerExit(Collider other)
        {
            _onTriggerExit?.Invoke(other);
        }
    }
}