using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TheSTAR.Utility;

namespace World
{
    public class Miner : MonoBehaviour
    {
        private int _animLTID = -1;
        private bool _isMining = false;
        private float _mineStrikePeriod = 1;
        private const float DefaultMineStrikeTime = 0.5f;

        private List<ResourceSource> _availableSource;
        private ResourceSource _currentSource;
        private TimeCycleControl _miningCycle;
        private Transform _visualTran;

        public ResourceSource CurrentSource => _currentSource;
        public bool IsMining => _isMining;

        public event Action OnStopMiningEvent;

        public void Init(Transform visualTran)
        {
            _visualTran = visualTran;
            _availableSource = new ();
        }

        public void AddAvailableSource(ResourceSource source)
        {
            if (_availableSource.Contains(source)) return;
            _availableSource.Add(source);
        }

        public void RemoveAvailableSource(ResourceSource source)
        {
            if (!_availableSource.Contains(source)) return;
            _availableSource.Remove(source);
        }

        public void StartMining(ResourceSource source)
        {
            BreakAnim();
            _currentSource = source;
            var miningData = source.SourceData.MiningData;
            _mineStrikePeriod = miningData.MiningPeriod;

            _isMining = true;

            if (_miningCycle != null) _miningCycle.Stop();

            _miningCycle = TimeUtility.DoWhile(() => _isMining, _mineStrikePeriod, DoMineStrike);
        }

        public void StopMining(ResourceSource rs)
        {
            if (_currentSource != rs) return;

            _currentSource = null;
            _isMining = false;
            BreakAnim();

            if (_miningCycle != null) _miningCycle.Stop();

            OnStopMiningEvent?.Invoke();
        }

        private void DoMineStrike()
        {
            BreakAnim();

            var animTimeMultiply = _mineStrikePeriod > DefaultMineStrikeTime ? 1 : (_mineStrikePeriod / DefaultMineStrikeTime * 0.9f);

            _animLTID =
            LeanTween.scaleY(_visualTran.gameObject, 1.2f, DefaultMineStrikeTime * 0.8f * animTimeMultiply).setOnComplete(() =>
            {
                _animLTID =
                    LeanTween.scaleY(_visualTran.gameObject, 1f, DefaultMineStrikeTime * 0.2f * animTimeMultiply).setOnComplete(() => _currentSource.TakeHit()).id;
            }).id;
        }

        private void BreakAnim()
        {
            if (_animLTID == -1) return;
            LeanTween.cancel(_animLTID);
            _visualTran.localScale = Vector3.one;
            _animLTID = -1;
        }

        public void RetryInteract(ResourceSource source)
        {
            if (_isMining || !_availableSource.Contains(source)) return;

            RetryInteract(out _);
        }

        public void RetryInteract(out bool successful)
        {
            foreach (var s in _availableSource)
            {
                if (s == null || !s.CanInteract) continue;
                StartMining(s);
                successful = true;
                return;
            }

            successful = false;
        }
    }
}