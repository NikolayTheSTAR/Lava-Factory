using System;
using UnityEngine;

namespace World
{
    public abstract class CiObject : MonoBehaviour, ICollisionInteractable
    {
        public event Action OnEnterEvent;
        public void OnEnter() => OnEnterEvent?.Invoke();

        public abstract CiType GetCiType { get; }
        public abstract Collider Col { get; }
        public abstract bool CanInteract { get; }
    }

    public enum CiType
    {
        Source,
        Factory
    }
}