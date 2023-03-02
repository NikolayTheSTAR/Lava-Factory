using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace World
{
    public class ResourceSource : MonoBehaviour
    {
        [SerializeField] private SourceType sourceType;

        private int animLTID = -1;
        private Action<ResourceSource> _dropItemAction;

        public SourceType SourceType => sourceType;

        public void Init(Action<ResourceSource> dropItemAction)
        {
            _dropItemAction = dropItemAction;
        }
        
        public void TakeHit()
        {
            if (animLTID != -1)
            {
                LeanTween.cancel(animLTID);
                transform.localScale = Vector3.one;
            }
            
            animLTID =
            LeanTween.scaleY(gameObject, 0.85f, 0.1f).setOnComplete(() =>
            {
                animLTID =
                LeanTween.scaleY(gameObject, 1f, 0.2f).id;
            }).id;

            _dropItemAction?.Invoke(this);
        }
    }

    public enum SourceType
    {
        AppleTree
    }
}