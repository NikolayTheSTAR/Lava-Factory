using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using UnityEngine;
using Tutorial;

namespace World
{
    [Serializable]
    public class Factory : CiObject, IDropReceiver, IDropSender
    {
        [SerializeField] private Collider ciCollider;
        [SerializeField] private FactoryType factoryType;

        public override CiType GetCiType => CiType.Factory;
        public override bool CanInteract => !_isSending && (_itemsInStorageCount + _itemsOnWayCount) < _factoryData.NeededFromItemCount;
        public override Collider Col => ciCollider;

        public FactoryType FactoryType => factoryType;

        private int _index;
        private FactoryData _factoryData;
        private int _itemsInStorageCount = 0;
        private int _itemsOnWayCount = 0;
        private bool _isSending = false;

        public FactoryData FactoryData => _factoryData;
        
        private Action<IDropSender, ItemType> _dropItemAction;

        public event Action<int, int> OnAddItemToStorageEvent;
        public event Action<int> OnEmptyStorageEvent;

        public void Init(int index, FactoryData factoryData, Action<IDropSender, ItemType> dropItemAction, int itemsInStorageCount)
        {
            _index = index;
            _factoryData = factoryData;
            _dropItemAction = dropItemAction;
            _itemsInStorageCount = itemsInStorageCount;
        }

        private void AddNeededResource()
        {
            _itemsInStorageCount++;

            if (_itemsInStorageCount < _factoryData.NeededFromItemCount)
            {
                OnAddItemToStorageEvent?.Invoke(_index, 1);
                return;
            }
            
            _itemsInStorageCount = 0;
            OnEmptyStorageEvent?.Invoke(_index);
            _isSending = true;

            // wait for craft

            WaitingUtility.Wait(_factoryData.CraftTime, () =>
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
        WheatFactory,
        IronFactory,
        CrystalFactory
    }   
}