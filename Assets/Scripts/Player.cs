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
    [SerializeField] private EntranceTrigger trigger;

    private float 
        _mineStrikePeriod = 1,
        _dropToFactoryPeriod = 1;
    
    private bool 
        _isMoving = false, 
        _isMining = false, 
        _isTransaction = false;

    private TransactionsController _transactions;
    private List<ICollisionInteractable> _currentCIs;
    private ResourceSource _currentSource;
    private Factory _currentFactory;
    private Coroutine _mineCoroutine;
    private Coroutine _transactionCoroutine;
    private int _animLTID = -1;

    public delegate void OnStartMiningDelegate(SourceType sourceType, out SourceMiningData miningData);
    private OnStartMiningDelegate _onStartMining;
    private Action<Factory> _dropToFactoryAction;

    private const float DefaultMineStrikeTime = 0.5f;
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

    public void Init(TransactionsController transactions, OnStartMiningDelegate onStartMining, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
    {
        _transactions = transactions;
        trigger.Init(OnEnter, OnExit);
        _onStartMining = onStartMining;
        _dropToFactoryAction = dropToFactoryAction;
        _dropToFactoryPeriod = dropToFactoryPeriod;
        
        trigger.SetRadius(CharacterConfig.TriggerRadius);

        _currentCIs = new List<ICollisionInteractable>();
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
        
        _currentCIs.Add(ci);
        
        if (!ci.CanInteract) return;
        if (ci.Condition == ICICondition.None) ci.Interact(this);
    }
    
    private void OnExit(Collider other)
    {
        var ci = other.GetComponent<ICollisionInteractable>();
        if (ci == null) return;
        if (_currentCIs.Contains(ci)) _currentCIs.Remove(ci);
        
        ci.StopInteract(this);
    }

    private void OnStartMove()
    {
        _isMoving = true;

        foreach (var ci in _currentCIs)
        {
            if (ci == null || !ci.CanInteract) return;
            if (ci.Condition == ICICondition.PlayerIsStopped) ci.StopInteract(this);   
        }
    }
    
    private void OnStopMove()
    {
        _isMoving = false;

        foreach (var ci in _currentCIs)
        {
            if (ci == null || !ci.CanInteract) continue;
            if (ci.Condition != ICICondition.PlayerIsStopped) continue;
            ci.Interact(this);
            //Debug.Log("Должно произойти взаимодействие c " + (ci as Factory).name);
            return;
        }
    }

    #endregion
    
    #region Mining

    public void StartMining(ResourceSource source)
    {
        _currentSource = source;
        _onStartMining(source.SourceType, out var miningData);
        _mineStrikePeriod = miningData.MiningPeriod;
        
        _isMining = true;
        
        _mineCoroutine = StartCoroutine(MiningCor());
    }
        
    public void StopMining(ResourceSource rs)
    {
        _currentSource = null;
        _isMining = false;
        LeanTween.cancel(_animLTID);
        if (_mineCoroutine != null) StopCoroutine(_mineCoroutine);
        _animLTID = LeanTween.scaleY(gameObject, 1, DefaultMineStrikeTime).id;
        
        RetryInteract();
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
        if (_animLTID != -1) LeanTween.cancel(_animLTID);

        var animTimeMultiply = _mineStrikePeriod > DefaultMineStrikeTime ? 1 : (_mineStrikePeriod / DefaultMineStrikeTime * 0.9f);
        
        _animLTID =
            LeanTween.scaleY(gameObject, 1.2f, DefaultMineStrikeTime * 0.8f * animTimeMultiply).setOnComplete(() =>
            {
                _animLTID =
                    LeanTween.scaleY(gameObject, 1f, DefaultMineStrikeTime * 0.2f * animTimeMultiply).setOnComplete(() => _currentSource.TakeHit()).id;
            }).id;
    }

    public void RetryInteract()
    {
        foreach (var ci in _currentCIs)
        {
            if (ci == null || !ci.CanInteract) continue;
            
            // conditions
            if (ci.Condition == ICICondition.PlayerIsStopped && _isMoving) continue;
            
            // check for Factory
            if (ci is Factory f && !_transactions.CanStartTransaction(f)) continue;
            
            ci.Interact(this);
            return;
        }
    }
    
    #endregion

    #region Transactions

    public void StartTransaction(Factory factory)
    {
        _isTransaction = true;
        _currentFactory = factory;
        _transactionCoroutine = StartCoroutine(TransactionsCor());
    }
    
    public void StopTransaction()
    {
        _isTransaction = false;
        if (_transactionCoroutine != null) StopCoroutine(_transactionCoroutine);
        _currentFactory = null;
        
        RetryInteract();
    }
    
    private IEnumerator TransactionsCor()
    {
        while (_isTransaction)
        {
            if (_currentFactory.CanInteract) _dropToFactoryAction(_currentFactory);
            yield return new WaitForSeconds(_dropToFactoryPeriod);
        }
        yield return null;
    }

    #endregion

    public void OnStartReceiving() {}

    public void OnCompleteReceiving() {}
}