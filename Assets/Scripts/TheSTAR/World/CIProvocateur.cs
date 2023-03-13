using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class CIProvocateur : MonoBehaviour, ICIProvocateur
    {
        [SerializeField] private EntranceTrigger trigger;

        private ICollisionInteractable _currentCI;
        private List<ICollisionInteractable> _currentCIs;
        private TransactionsController _transactions;
        [Obsolete] private Player _player;

        public List<ICollisionInteractable> CurrentCIs => _currentCIs;


        public void Init(Player p, TransactionsController transactions, float radius)
        {
            _player = p;
            _transactions = transactions;
            _currentCIs = new List<ICollisionInteractable>();

            trigger.Init(OnEnter, OnExit);
            trigger.SetRadius(radius);
        }

        private void OnEnter(Collider other)
        {
            var ci = other.GetComponent<ICollisionInteractable>();
            if (ci == null) return;

            ci.OnEnter();

            _currentCIs.Add(ci);

            if (!ci.CanInteract) return;

            StartInteract(ci);
        }

        private void OnExit(Collider other)
        {
            var ci = other.GetComponent<ICollisionInteractable>();
            if (ci == null) return;
            if (_currentCIs.Contains(ci)) _currentCIs.Remove(ci);

            StopInteract(ci);
        }

        public void StartInteract(ICollisionInteractable ci)
        {
            _currentCI = ci;
            if (ci.CompareTag("Source")) _player.Miner.StartMining(ci.Col.GetComponent<ResourceSource>());
            else if (ci.CompareTag("Factory")) _player.Crafter.StartCraft(ci.Col.GetComponent<Factory>());
        }

        public void StopInteract()
        {
            if (_currentCI == null) return;
            StopInteract(_currentCI);
        }

        public void StopInteract(ICollisionInteractable ci)
        {
            if (ci.CompareTag("Source")) _player.Miner.StopMining(ci.Col.GetComponent<ResourceSource>());
            else if (ci.CompareTag("Factory")) _player.Crafter.StopCraft();
        }

        public void RetryInteract()
        {
            foreach (var ci in _currentCIs)
            {
                if (ci == null || !ci.CanInteract) continue;

                // conditions
                if (ci.CompareTag("Factory") && _player.IsMoving) continue;

                // check for Factory
                if (ci is Factory f && !_transactions.CanStartTransaction(f)) continue;

                StartInteract(ci);
                return;
            }
        }
    }
}