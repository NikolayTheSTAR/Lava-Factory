using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TheSTAR.Utility;

namespace World
{
    public class Crafter : MonoBehaviour
    {
        private float _dropToFactoryPeriod = 1;
        private bool _crafting = false;

        private Factory _currentFactory;
        private List<Factory> _availableFactories;
        private TimeCycleControl _craftCycle;
        private TransactionsController _transactions;

        private Action<Factory> _dropToFactoryAction;
        public event Action OnStopCraftEvent;

        public Factory CurrentFactory => _currentFactory;

        public void Init(TransactionsController transactions, float dropToFactoryPeriod, Action<Factory> dropToFactoryAction)
        {
            _transactions = transactions;
            _dropToFactoryPeriod = dropToFactoryPeriod;
            _dropToFactoryAction = dropToFactoryAction;
            _availableFactories = new ();
        }

        public void AddAvailableFactory(Factory factory)
        {
            if (_availableFactories.Contains(factory)) return;
            _availableFactories.Add(factory);
        }

        public void RemoveAvailableFactory(Factory factory)
        {
            if (!_availableFactories.Contains(factory)) return;
            _availableFactories.Remove(factory);
        }

        public void StartCraft(Factory factory)
        {
            _crafting = true;
            _currentFactory = factory;

            if (_craftCycle != null) _craftCycle.Stop();

            _craftCycle = TimeUtility.DoWhile(() => _crafting, _dropToFactoryPeriod, () =>
            {
                if (_currentFactory.CanInteract) _dropToFactoryAction(_currentFactory);
            });
        }

        public void StopCraft()
        {
            _crafting = false;

            if (_craftCycle != null) _craftCycle.Stop();

            _currentFactory = null;

            OnStopCraftEvent?.Invoke();
        }

        public void RetryInteract(out bool successful)
        {
            foreach (var f in _availableFactories)
            {
                if (f == null || !f.CanInteract || !_transactions.CanStartTransaction(f)) continue;
                StartCraft(f);
                successful = true;
                return;
            }

            successful = false;
        }
    }
}