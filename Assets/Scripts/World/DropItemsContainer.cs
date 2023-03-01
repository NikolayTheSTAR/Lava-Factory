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
        
        public void DropItemToPlayer(ResourceItem itemPrefab, Vector3 pos)
        {
            var item = Instantiate(itemPrefab, pos, Quaternion.identity, transform);
            item.transform.localScale = Vector3.zero;
            
            LeanTween.scale(item.gameObject, Vector3.one, 0.2f).setOnComplete(() =>
            {
                LeanTween.value(0, 1, DropWaitTime).setOnComplete(() =>
                {
                    LeanTween.move(item.gameObject, _playerDropReceiver.transform.position, FlyToReceiverTime).setOnComplete(() =>
                    {
                        Destroy(item.gameObject);
                    });
                });
            });
        }
    }

    public interface IDropReceiver
    {
        Transform transform { get; }
    }
}