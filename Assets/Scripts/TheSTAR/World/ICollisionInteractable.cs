using System;
using UnityEngine;

namespace World
{
    // объекты, с которыми возможно коллизионное взаимодействие
    public interface ICollisionInteractable
    {
        bool CompareTag(string tag);
        Collider Col { get; }
        bool CanInteract { get; }
        void OnEnter();
    }

    // объект, провоцирующий коллизионное взаимодействие
    public interface ICIProvocateur
    {
        void StartInteract(ICollisionInteractable ci);
        void StopInteract(ICollisionInteractable ci);
        void RetryInteract();
    }
}