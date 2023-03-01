using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace World
{
    public class ResourceSource : MonoBehaviour
    {
        [SerializeField] private ResourceItem itemPrefab;

        private int animLTID = -1;
        private Action<ResourceItem, Vector3> _dropItemAction;

        public void Init(Action<ResourceItem, Vector3> dropItemAction)
        {
            _dropItemAction = dropItemAction;
        }
        
        public void TakeHit()
        {
            if (animLTID != -1) LeanTween.cancel(animLTID);
            
            animLTID =
            LeanTween.scaleY(gameObject, 0.85f, 0.1f).setOnComplete(() =>
            {
                animLTID =
                LeanTween.scaleY(gameObject, 1f, 0.2f).id;
            }).id;

            _dropItemAction?.Invoke(itemPrefab, transform.position + new Vector3(0, 3 ,-2));
        }
    }
}