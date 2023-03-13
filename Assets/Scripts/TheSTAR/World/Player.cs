using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using TheSTAR.Input;
using UnityEngine;
using UnityEngine.AI;
using World;

public class Player : MonoBehaviour, ICameraFocusable, IJoystickControlled, IDropReceiver, ICIProvocateur
{
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private EntranceTrigger trigger;
    [SerializeField] private Transform visualTran;
    [SerializeField] private Miner miner;
    [SerializeField] private Crafter crafter;
    
    private bool _isMoving = false;

    private TransactionsController _transactions;
    private List<ICollisionInteractable> _currentCIs;
    private ICollisionInteractable _currentCI;

    public event Action OnMoveEvent;

    
    private const string CharacterConfigPath = "Configs/CharacterConfig";
    
    private CharacterConfig _characterConfig;

    private CharacterConfig CharacterConfig
    {
        get
        {
            if (_characterConfig == null) _characterConfig = Resources.Load<CharacterConfig>(CharacterConfigPath);
            return _characterConfig;
        }
    }

    public void Init(TransactionsController transactions, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
    {
        _transactions = transactions;
        trigger.Init(OnEnter, OnExit);        
        trigger.SetRadius(CharacterConfig.TriggerRadius);

        _currentCIs = new List<ICollisionInteractable>();

        miner.Init(visualTran);
        miner.OnStopMiningEvent += RetryInteract;

        crafter.Init(dropToFactoryPeriod, dropToFactoryAction);
        crafter.OnStopCraftEvent += RetryInteract;
    }

    #region Logic Enter

    public void JoystickInput(Vector2 input)
    {
        Vector3 finalMoveDirection;

        if (input.x != 0 || input.y != 0)
        {
            var tempMoveDirection = new Vector3(input.x, 0, input.y);
            finalMoveDirection = transform.position + tempMoveDirection;
            
            if (!_isMoving) OnStartMove();

            OnMove();

            // rotate

            var lookRotation = Quaternion.LookRotation(tempMoveDirection);
            var euler = lookRotation.eulerAngles;
            visualTran.rotation = Quaternion.Euler(0, euler.y, 0);
        }
        else
        {
            finalMoveDirection = transform.position;
            
            if (_isMoving) OnStopMove();
        }

        meshAgent.SetDestination(finalMoveDirection);
    }

    private void OnEnter(Collider other)
    {
        var ci = other.GetComponent<ICollisionInteractable>();
        if (ci == null) return;
        
        ci.OnEnter();

        _currentCIs.Add(ci);
        
        if (!ci.CanInteract) return;
        if (!ci.CompareTag("Factory")) StartInteract(ci);
    }

    private void OnExit(Collider other)
    {
        var ci = other.GetComponent<ICollisionInteractable>();
        if (ci == null) return;
        if (_currentCIs.Contains(ci)) _currentCIs.Remove(ci);
        
        StopInteract(ci);
    }

    private void OnStartMove()
    {
        _isMoving = true;

        foreach (var ci in _currentCIs)
        {
            if (ci == null || !ci.CanInteract) return;
            if (ci.CompareTag("Factory")) StopInteract(ci);   
        }
    }
    
    private void OnMove() => OnMoveEvent?.Invoke();
    
    private void OnStopMove()
    {
        _isMoving = false;

        foreach (var ci in _currentCIs)
        {
            if (ci == null || !ci.CanInteract) continue;
            if (!ci.CompareTag("Factory")) continue;

            StartInteract(ci);

            return;
        }
    }

    #endregion

    #region CI Provocateur

    public void StartInteract(ICollisionInteractable ci)
    {
        _currentCI = ci;
        if (ci.CompareTag("Source")) miner.StartMining(ci.Col.GetComponent<ResourceSource>());
        else if (ci.CompareTag("Factory")) crafter.StartCraft(ci.Col.GetComponent<Factory>());
    }

    public void StopInteract()
    {
        if (_currentCI == null) return;
        StopInteract(_currentCI);
    }

    public void StopInteract(ICollisionInteractable ci)
    {
        if (ci.CompareTag("Source")) miner.StopMining(ci.Col.GetComponent<ResourceSource>());
        else if (ci.CompareTag("Factory")) crafter.StopCraft();
    }

    public void RetryInteract()
    {
        foreach (var ci in _currentCIs)
        {
            if (ci == null || !ci.CanInteract) continue;

            // conditions
            if (ci.CompareTag("Factory") && _isMoving) continue;

            // check for Factory
            if (ci is Factory f && !_transactions.CanStartTransaction(f)) continue;

            StartInteract(ci);
            return;
        }
    }

    #endregion

    public void OnStartReceiving() {}

    public void OnCompleteReceiving() {}
}