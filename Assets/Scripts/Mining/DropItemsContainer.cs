using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using World;
using Random = UnityEngine.Random;

namespace Mining
{ 
    public class DropItemsContainer : MonoBehaviour
    {
        private IDropReceiver _playerDropReceiver;
        private MiningController _miningController;

        private const float DropWaitTime = 0.2f;
        private const float FlyToReceiverTime = 0.5f;

        private Dictionary<ItemType, List<ResourceItem>> _itemPools;

        public void Init(MiningController miningController, IDropReceiver playerDropReceiver)
        {
            _playerDropReceiver = playerDropReceiver;
            _itemPools = new Dictionary<ItemType, List<ResourceItem>>();
            _miningController = miningController;
        }
        
        private Vector3 _standardDropOffset = new Vector3(0, 3, -2);
        private float _randomOffsetRange = 0.4f;

        public void DropFromSource(ResourceSource source)
        {
            var miningData =_miningController.SourcesConfig.SourceDatas[(int)source.SourceType].MiningData;

            for (var i = 0; i < miningData.OneHitDropCount; i++)
            {
                var dropItemType = _miningController.SourcesConfig.SourceDatas[(int)source.SourceType].DropItemType;
                var offset = _standardDropOffset +
                             new Vector3(
                                 Random.Range(-_randomOffsetRange, _randomOffsetRange), 
                                 Random.Range(-_randomOffsetRange, _randomOffsetRange),
                                 Random.Range(-_randomOffsetRange, _randomOffsetRange));
            
                DropItemToPlayer(dropItemType, source.transform.position + offset);
            }
        }
        
        private void DropItemToPlayer(ItemType itemType, Vector3 startPos)
        {
            var item = GetItemFromPool(itemType, startPos);
            item.transform.localScale = Vector3.zero;
            
            LeanTween.scale(item.gameObject, Vector3.one, 0.2f).setOnComplete(() => { LeanTween.value(0, 1, DropWaitTime).setOnComplete(FlyToPlayer);});

            void FlyToPlayer()
            {
                LeanTween.value(0, 1, FlyToReceiverTime).setOnUpdate((value) =>
                {
                    var difference = _playerDropReceiver.transform.position - startPos;
                    item.transform.position = startPos + value * (difference);
                    
                    // physic imitation
                    var impulseForce = _miningController.ItemsConfig.Items[(int)itemType].PhysicalImpulse;
                    var dopValueY = Math.Abs((value * value - value) * impulseForce * difference.x);
                    item.transform.position += new Vector3(0, dopValueY, 0);

                }) .setOnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                });
            }
        }

        private ResourceItem GetItemFromPool(ItemType itemType, Vector3 startPos, bool autoActivate = true)
        {
            if (_itemPools.ContainsKey(itemType))
            {
                var pool = _itemPools[itemType];
                var itemInPool = pool?.Find(info => !info.gameObject.activeSelf);
                if (itemInPool != null)
                {
                    if (autoActivate) itemInPool.gameObject.SetActive(true);
                    itemInPool.transform.position = startPos;
                    return itemInPool;
                }
                
                var newItem = CreateItem();
                pool.Add(newItem);
                return newItem;
            }
            else
            {
                var newItem = CreateItem();
                _itemPools.Add(itemType, new List<ResourceItem>(){newItem});
                return newItem;
            }

            ResourceItem CreateItem() => Instantiate(_miningController.GetResourceItemPrefab(itemType), startPos, quaternion.identity, transform);
        }
    }

    public interface IDropReceiver
    {
        Transform transform { get; }
    }
}