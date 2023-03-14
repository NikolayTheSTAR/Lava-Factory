using System;
using UnityEngine;

namespace World
{
    [Obsolete]
    // объекты, с которыми возможно коллизионное взаимодействие
    public interface ICollisionInteractable
    {
        public CiType GetCiType { get; }
        //[Obsolete] bool CompareTag(string tag); // из за существования этого метода повсеместно возникает проверка на тег, что являеся тяжеловатым, нужно придумать как отойти
        [Obsolete] Collider Col { get; }
        bool CanInteract { get; }
        void OnEnter();
    }

    [Obsolete]
    // объект, провоцирующий коллизионное взаимодействие
    public interface ICIProvocateur
    {
        void StartInteract(ICollisionInteractable ci);
        void StopInteract();
        void StopInteract(ICollisionInteractable ci);
        void RetryInteract();
    }
}