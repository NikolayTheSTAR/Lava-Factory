using System.Collections;
using System;
using UnityEngine;

namespace World
{
    public class Crafter : MonoBehaviour
    {
        private float _dropToFactoryPeriod = 1;
        private bool _crafting = false;

        private Factory _currentFactory;
        private Coroutine _craftCoroutine;

        private Action<Factory> _dropToFactoryAction;
        public event Action OnStopCraftEvent;

        public void Init(float dropToFactoryPeriod, Action<Factory> dropToFactoryAction)
        {
            _dropToFactoryPeriod = dropToFactoryPeriod;
            _dropToFactoryAction = dropToFactoryAction;
        }

        public void StartCraft(Factory factory)
        {
            _crafting = true;
            _currentFactory = factory;

            if (_craftCoroutine != null) StopCoroutine(_craftCoroutine);
            _craftCoroutine = StartCoroutine(CraftCor());
        }

        public void StopCraft()
        {
            _crafting = false;
            if (_craftCoroutine != null) StopCoroutine(_craftCoroutine);
            _currentFactory = null;

            OnStopCraftEvent?.Invoke();
        }

        private IEnumerator CraftCor()
        {
            while (_crafting)
            {
                if (_currentFactory.CanInteract) _dropToFactoryAction(_currentFactory);
                yield return new WaitForSeconds(_dropToFactoryPeriod);
            }
            yield return null;
        }
    }
}