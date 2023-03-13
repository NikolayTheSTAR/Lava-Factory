using System.Collections;
using System;
using UnityEngine;

namespace World
{
    public class Miner : Role
    {
        private int _animLTID = -1;
        private bool _isMining = false;
        private float _mineStrikePeriod = 1;
        private const float DefaultMineStrikeTime = 0.5f;

        private ResourceSource _currentSource;
        private Coroutine _mineCoroutine;
        private Transform _visualTran;

        public event Action OnStopMiningEvent;

        public void Init(Transform visualTran)
        {
            _visualTran = visualTran;
        }

        public void StartMining(ResourceSource source)
        {
            BreakAnim();
            _currentSource = source;
            var miningData = source.SourceData.MiningData;
            _mineStrikePeriod = miningData.MiningPeriod;

            _isMining = true;

            if (_mineCoroutine != null) StopCoroutine(_mineCoroutine);
            _mineCoroutine = StartCoroutine(MiningCor());
        }

        public void StopMining(ResourceSource rs)
        {
            if (_currentSource != rs) return;

            _currentSource = null;
            _isMining = false;
            BreakAnim();

            if (_mineCoroutine != null) StopCoroutine(_mineCoroutine);

            OnStopMiningEvent?.Invoke();
        }

        private IEnumerator MiningCor()
        {
            while (_isMining)
            {
                DoMineStrike();
                yield return new WaitForSeconds(_mineStrikePeriod);
            }
            yield return null;
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
    }
}