using System;
using System.Collections.Generic;
using Configs;
using Unity.Mathematics;
using UnityEngine;

namespace World
{
    public class DropItemsContainer : MonoBehaviour
    {
        private IDropReceiver _playerDropReceiver;
        
        private readonly float DropWaitTime = 0.2f;
        private readonly float FlyToReceiverTime = 0.5f;

        private Dictionary<ItemType, List<ResourceItem>> _itemPools;
        private Dictionary<ItemType, ResourceItem> _loadedItemPrefabs;

        private const string SourcesLoadPath = "Configs/SourcesConfig";

        private SourcesConfig sourceConfig;

        public SourcesConfig SourcesConfig
        {
            get
            {
                if (sourceConfig == null) sourceConfig = Resources.Load<SourcesConfig>(SourcesLoadPath);

                return sourceConfig;
            }
        }

        private string ItemLoadPath(ItemType itemType) => $"Items/{itemType.ToString()}";
        
        public void Init(IDropReceiver playerDropReceiver)
        {
            _playerDropReceiver = playerDropReceiver;
            _itemPools = new Dictionary<ItemType, List<ResourceItem>>();
            _loadedItemPrefabs = new Dictionary<ItemType, ResourceItem>();
        }

        public void DropItemFromSource(ResourceSource source)
        {
            var dropItemType = SourcesConfig.SourceDatas[(int)source.SourceType].DropItemType;
            var offset = new Vector3(0, 3, -2);
            
            DropItemToPlayer(dropItemType, source.transform.position + offset);
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
                    var dopValueY = Math.Abs((value * value - value) * 2 * difference.x);
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

            ResourceItem CreateItem() => Instantiate(GetResourceItemPrefab(itemType), startPos, quaternion.identity, transform);
        }

        private ResourceItem GetResourceItemPrefab(ItemType itemType)
        {
            if (_loadedItemPrefabs.ContainsKey(itemType)) return _loadedItemPrefabs[itemType];
            
            var loadedItem = Resources.Load<ResourceItem>(ItemLoadPath(itemType));
            _loadedItemPrefabs.Add(itemType, loadedItem);
            return loadedItem;
        }
    }

    public interface IDropReceiver
    {
        Transform transform { get; }
    }
}