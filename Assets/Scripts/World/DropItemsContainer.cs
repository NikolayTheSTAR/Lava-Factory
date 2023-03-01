using System;
using UnityEngine;

namespace World
{
    public class DropItemsContainer : MonoBehaviour
    {
        private IDropReceiver _playerDropReceiver;

        [SerializeField] private float DropWaitTime = 0.2f;
        [SerializeField] private float FlyToReceiverTime = 0.5f;
        
        public void Init(IDropReceiver playerDropReceiver)
        {
            _playerDropReceiver = playerDropReceiver;
        }
        
        public void DropItemToPlayer(ResourceItem itemPrefab, Vector3 startPos)
        {
            var item = Instantiate(itemPrefab, startPos, Quaternion.identity, transform);
            item.transform.localScale = Vector3.zero;
            
            LeanTween.scale(item.gameObject, Vector3.one, 0.2f).setOnComplete(() => { LeanTween.value(0, 1, DropWaitTime).setOnComplete(FlyToPlayer);});

            void FlyToPlayer()
            {
                //LeanTween.move(item.gameObject, _playerDropReceiver.transform.position, FlyToReceiverTime)
                LeanTween.value(0, 1, FlyToReceiverTime).setOnUpdate((value) =>
                {
                    var difference = _playerDropReceiver.transform.position - startPos;
                    item.transform.position = startPos + value * (difference);

                    var dopValueY_Use = (value * value - value)*2;
                    var dopValueY = Math.Abs(dopValueY_Use * difference.x);
                    item.transform.position += new Vector3(0, dopValueY, 0);

                }) .setOnComplete(() =>
                {
                    Destroy(item.gameObject);
                });
            }
        }
    }

    public interface IDropReceiver
    {
        Transform transform { get; }
    }
}