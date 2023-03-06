using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using UnityEngine;

namespace World
{
    public class Factory : MonoBehaviour, ICollisionInteractable, IDropReceiver, IDropSender
    {
        [SerializeField] private FactoryType factoryType;

        public bool CanInteract => !_isSending && (_itemsInStorageCount + _itemsOnWayCount) < _factoryData.NeededFromItemCount;
        public ICICondition Condition => ICICondition.PlayerIsStopped;
        public FactoryType FactoryType => factoryType;

        private FactoryData _factoryData;
        private int _itemsInStorageCount = 0;
        private int _itemsOnWayCount = 0;
        private bool _isSending = false;
        
        private Action<IDropSender, ItemType> _dropItemAction;

        public void Init(FactoryData factoryData, Action<IDropSender, ItemType> dropItemAction)
        {
            _factoryData = factoryData;
            _dropItemAction = dropItemAction;
        }
        
        public void Interact(Player p)
        {
            p.StartTransaction(this);
        }

        public void StopInteract(Player p)
        {
            p.StopTransaction();
        }

        private void AddNeededResource()
        {
            _itemsInStorageCount++;
            if (_itemsInStorageCount < _factoryData.NeededFromItemCount) return;
            
            _itemsInStorageCount -= _factoryData.NeededFromItemCount;
            _isSending = true;
            
            // wait for craft
            
            LeanTween.value(0, 1, _factoryData.CraftTime).setOnComplete(() =>
            {
                for (var i = 0; i < _factoryData.ResultToItemCount; i++) 
                    _dropItemAction(this, _factoryData.ToItemType);
            });
        }

        public void OnCompleteDrop()
        {
            _isSending = false;
        }

        public void OnStartReceiving()
        {
            _itemsOnWayCount++;
        }

        public void OnCompleteReceiving()
        {
            _itemsOnWayCount--;
            AddNeededResource();
        }
    }

    public enum FactoryType
    {
        AppleFactory,
        LogFactory,
        WheatFactory
    }   
}