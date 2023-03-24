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
    [SerializeField] private EntranceTrigger trigger;

    private Miner miner;
    private Crafter crafter;

    private bool _isMoving = false;
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
        miner = new();
        miner.Init(visualTran);
        miner.OnStopMiningEvent += RetryInteract;

        crafter = new();
        crafter.Init(transactions, dropToFactoryPeriod, dropToFactoryAction);
        crafter.OnStopCraftEvent += RetryInteract;

        trigger.Init(OnEnter, OnExit);
        trigger.SetRadius(CharacterConfig.TriggerRadius);
    }

    #region Move

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

        if (crafter.CurrentFactory == null) return;
        StopInteract(crafter.CurrentFactory);
    }
    
    private void OnMove() => OnMoveEvent?.Invoke();
    
    private void OnStopMove()
    {
        _isMoving = false;

        crafter.RetryInteract(out _);
    }

    #endregion

    #region Interactions

    private void OnEnter(Collider other)
    {
        if (other.CompareTag("Source"))
        {
            var s = other.GetComponent<ResourceSource>();
            miner.AddAvailableSource(s);
            s.OnEnter();

            if (miner.IsMining) return;
            if (!s.CanInteract) return;

            StartInteract(s);
        }
        else if (other.CompareTag("Factory"))
        {
            var f = other.GetComponent<Factory>();
            crafter.AddAvailableFactory(f);
            f.OnEnter();

            if (!f.CanInteract) return;
            if (!_isMoving) StartInteract(f);
        }
    }

    private void OnExit(Collider other)
    {
        if (other.CompareTag("Source"))
        {
            var s = other.GetComponent<ResourceSource>();
            miner.RemoveAvailableSource(s);

            StopInteract(s);
        }
        else if (other.CompareTag("Factory"))
        {
            var f = other.GetComponent<Factory>();
            crafter.RemoveAvailableFactory(f);

            StopInteract(f);
        }
    }

    private void StartInteract(ResourceSource source) => miner.StartMining(source);
    private void StartInteract(Factory factory) => crafter.StartCraft(factory);

    public void StopInteract()
    {
        if (miner.CurrentSource != null) StopInteract(miner.CurrentSource);
        if (crafter.CurrentFactory != null) StopInteract(crafter.CurrentFactory);
    }

    private void StopInteract(ResourceSource source) => miner.StopMining(source);
    private void StopInteract(Factory factory) => crafter.StopCraft();

    public void RetryInteract()
    {
        miner.RetryInteract(out bool successful);
        if (successful) return;

        if (!_isMoving) crafter.RetryInteract(out _);
    }

    public void RetryInteract(ResourceSource source) => miner.RetryInteract(source);

    #endregion

    public void OnStartReceiving() {}

    public void OnCompleteReceiving() {}
}