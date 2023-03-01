using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace World
{
    public class DropItemsContainer : MonoBehaviour
    {
        private IDropReceiver _playerDropReceiver;

        [SerializeField] private ResourceItem testItemPrefab;
        [SerializeField] private float DropWaitTime = 0.2f;
        [SerializeField] private float FlyToReceiverTime = 0.5f;

        private Dictionary<ItemType, List<ResourceItem>> itemPools;

        public void Init(IDropReceiver playerDropReceiver)
        {
            _playerDropReceiver = playerDropReceiver;
            itemPools = new Dictionary<ItemType, List<ResourceItem>>();
        }
        
        public void DropItemToPlayer(ItemType itemType, Vector3 startPos)
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
            if (itemPools.ContainsKey(itemType))
            {
                var pool = itemPools[itemType];
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
                itemPools.Add(itemType, new List<ResourceItem>(){newItem});
                return newItem;
            }

            ResourceItem CreateItem() => Instantiate(testItemPrefab, startPos, quaternion.identity, transform);
        }
    }

    public interface IDropReceiver
    {
        Transform transform { get; }
    }
}