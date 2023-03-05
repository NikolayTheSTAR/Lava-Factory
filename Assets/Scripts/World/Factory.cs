using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class Factory : MonoBehaviour, ICollisionInteractable
    {
        [SerializeField] private FactoryType factoryType;

        public bool CanInteract => true;
        public void Interact(Player p)
        {
            
        }

        public void StopInteract(Player p)
        {
            
        }
    }

    public enum FactoryType
    {
        AppleFactory,
        LogFactory,
        WheatFactory
    }   
}