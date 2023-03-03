using System;
using Configs;
using UnityEngine;
using UnityEngine.UIElements;

namespace World
{
    public class ResourceSource : MonoBehaviour
    {
        [SerializeField] private SourceType sourceType;
        [SerializeField] private GameObject prolificVisual;
        [SerializeField] private GameObject emptyVisual;

        private int _animLTID = -1;
        private Action<ResourceSource> _dropItemAction;
        private Action<ResourceSource> _onEmptying;
        private Action _onRecovery;
        private int _health = 1;
        private SourceMiningData _miningData;
        
        public bool IsEmpty { get; private set; }

        public SourceType SourceType => sourceType;
        public SourceMiningData MiningData => _miningData;

        public void Init(SourceMiningData miningData, Action<ResourceSource> dropItemAction, Action<ResourceSource> onEmptying, Action onRecovery)
        {
            _miningData = miningData;
            _dropItemAction = dropItemAction;
            _onEmptying = onEmptying;
            _onRecovery = onRecovery;

            _health = miningData.MaxHitsCount;
        }
        
        public void TakeHit()
        {
            if (_health <= 0) return;

            _health--;
            
            if (_animLTID != -1)
            {
                LeanTween.cancel(_animLTID);
                transform.localScale = Vector3.one;
            }
            
            _animLTID =
            LeanTween.scaleY(gameObject, 0.85f, 0.1f).setOnComplete(() =>
            {
                _animLTID =
                LeanTween.scaleY(gameObject, 1f, 0.2f).id;
            }).id;

            _dropItemAction?.Invoke(this);
            
            // emptying
            if (_health <= 0) Empty();
        }

        private void Empty()
        {
            prolificVisual.SetActive(false);
            emptyVisual.SetActive(true);
            IsEmpty = true;
            _onEmptying?.Invoke(this);
        }

        public void Recovery()
        {
            prolificVisual.SetActive(true);
            emptyVisual.SetActive(false);
            IsEmpty = false;
            _health = _miningData.MaxHitsCount;
            _onRecovery?.Invoke();
        }
    }

    public enum SourceType
    {
        AppleTree
    }
}