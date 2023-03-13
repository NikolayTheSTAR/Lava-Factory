using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using TheSTAR.Input;
using UnityEngine;
using UnityEngine.AI;
using World;

public class Player : MonoBehaviour, ICameraFocusable, IJoystickControlled, IDropReceiver
{
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private Transform visualTran;

    [Header("Modules")]
    [SerializeField] private Miner miner;
    [SerializeField] private Crafter crafter;
    [SerializeField] private CIProvocateur ciProvocateur;
    
    private bool _isMoving = false;
    private TransactionsController _transactions;
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

    public bool IsMoving => _isMoving;
    public Miner Miner => miner;
    public Crafter Crafter => crafter;

    public void Init(TransactionsController transactions, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
    {
        _transactions = transactions;
        

        miner.Init(visualTran);
        miner.OnStopMiningEvent += ciProvocateur.RetryInteract;

        crafter.Init(dropToFactoryPeriod, dropToFactoryAction);
        crafter.OnStopCraftEvent += ciProvocateur.RetryInteract;

        ciProvocateur.Init(this, transactions, CharacterConfig.TriggerRadius);
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

    private void OnStartMove()
    {
        _isMoving = true;

        foreach (var ci in ciProvocateur.CurrentCIs)
        {
            if (ci == null || !ci.CanInteract) return;
            if (ci.CompareTag("Factory")) ciProvocateur.StopInteract(ci);   
        }
    }
    
    private void OnMove() => OnMoveEvent?.Invoke();
    
    private void OnStopMove()
    {
        _isMoving = false;

        foreach (var ci in ciProvocateur.CurrentCIs)
        {
            if (ci == null || !ci.CanInteract) continue;
            if (!ci.CompareTag("Factory")) continue;

            ciProvocateur.StartInteract(ci);

            return;
        }
    }

    #endregion

    public void StopInteract()
    {
        ciProvocateur.StopInteract();
    }

    public void RetryInteract()
    {
        ciProvocateur.RetryInteract();
    }

    public void OnStartReceiving() {}

    public void OnCompleteReceiving() {}
}