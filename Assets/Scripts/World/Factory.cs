using System;
using System.Collections;
using System.Collections.Generic;
using Mining;
using UnityEngine;

namespace World
{
    public class Factory : MonoBehaviour, ICollisionInteractable, IDropReceiver
    {
        [SerializeField] private FactoryType factoryType;

        public bool CanInteract => true;
        public ICICondition Condition => ICICondition.PlayerIsStopped;
        public FactoryType FactoryType => factoryType;

        //private Action<Factory> _startTransactionAction;

        public void Init()
        {
            
        }
        
        public void Interact(Player p)
        {
            p.StartTransaction(this);
        }

        public void StopInteract(Player p)
        {
            p.StopTransaction();
        }
    }

    public enum FactoryType
    {
        AppleFactory,
        LogFactory,
        WheatFactory
    }   
}