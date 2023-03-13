using System;
using UnityEngine;

namespace World
{
    [Obsolete]
    public abstract class CiObject : MonoBehaviour, ICollisionInteractable
    {
        bool ICollisionInteractable.CompareTag(string value) => CompareTag(value);

        public event Action OnEnterEvent;
        public void OnEnter() => OnEnterEvent?.Invoke();

        public abstract Collider Col { get; }
        public abstract bool CanInteract { get; }
    }
}