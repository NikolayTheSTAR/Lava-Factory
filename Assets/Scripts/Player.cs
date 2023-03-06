using System;
using System.Collections;
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
    
    private float _mineStrikePeriod = 1;
    private float _dropToFactoryPeriod = 1;

    private const float DefaultMineStrikeTime = 0.5f;

    private bool _isMoving = false;
    private bool _isMining = false;
    private bool _isTransaction = false;
    
    private ICollisionInteractable _currentCollisionInteractable;
    private ResourceSource _currentSource;
    private Factory _currentFactory;
    private Coroutine _mineCoroutine;
    private Coroutine _transactionCoroutine;
    private int _animLTID = -1;

    public delegate void OnStartMiningDelegate(SourceType sourceType, out SourceMiningData miningData);
    private OnStartMiningDelegate _onStartMining;
    private Action<Factory> _dropToFactoryAction;

    private const string CharacterConfigPath = "Configs/CharacterConfig";
    private CharacterConfig _characterConfig;

    public CharacterConfig CharacterConfig
    {
        get
        {
            if (_characterConfig == null) _characterConfig = Resources.Load<CharacterConfig>(CharacterConfigPath);
            return _characterConfig;
        }
    }

    public void Init(OnStartMiningDelegate onStartMining, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
    {
        trigger.Init(OnEnter, OnExit);
        _onStartMining = onStartMining;
        _dropToFactoryAction = dropToFactoryAction;
        _dropToFactoryPeriod = dropToFactoryPeriod;
        
        trigger.SetRadius(CharacterConfig.TriggerRadius);
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
        _currentCollisionInteractable = ci;
        if (!ci.CanInteract) return;
        if (ci.Condition == ICICondition.None) ci.Interact(this);
    }
    
    private void OnExit(Collider other)
    {
        var ci = other.GetComponent<ICollisionInteractable>();
        if (ci == null) return;
        if (_currentCollisionInteractable == ci) _currentCollisionInteractable = null;
        ci.StopInteract(this);
    }

    private void OnStartMove()
    {
        _isMoving = true;
        if (_currentCollisionInteractable == null || !_currentCollisionInteractable.CanInteract) return;
        if (_currentCollisionInteractable.Condition == ICICondition.PlayerIsStopped) _currentCollisionInteractable.StopInteract(this);
    }
    
    private void OnStopMove()
    {
        _isMoving = false;
        if (_currentCollisionInteractable == null || !_currentCollisionInteractable.CanInteract) return;
        if (_currentCollisionInteractable.Condition == ICICondition.PlayerIsStopped) _currentCollisionInteractable.Interact(this);
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
        
    public void StopMining()
    {
        _isMining = false;
        LeanTween.cancel(_animLTID);
        
        if (_mineCoroutine != null) StopCoroutine(_mineCoroutine);

        _animLTID =
            LeanTween.scaleY(gameObject, 1, DefaultMineStrikeTime).id;

        _currentSource = null;
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
        if (_isMining || _currentCollisionInteractable == null || !_currentCollisionInteractable.CanInteract) return;

        _currentCollisionInteractable.Interact(this);
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

    public void OnStartReceiving()
    {
        
    }

    public void OnCompleteReceiving()
    {
        // nothing
    }
}