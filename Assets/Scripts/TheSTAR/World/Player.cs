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

    private Action<Factory> _dropToFactoryAction;
    
    public event Action OnMoveEvent;

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

    public void Init(TransactionsController transactions, Action<Factory> dropToFactoryAction, float dropToFactoryPeriod)
    {
        _transactions = transactions;
        trigger.Init(OnEnter, OnExit);
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
        if (ci.CompareTag("Source")) StartMining(ci.Col.GetComponent<ResourceSource>());
        else if (ci.CompareTag("Factory")) StartCraft(ci.Col.GetComponent<Factory>());
    }

    public void StopInteract(ICollisionInteractable ci)
    {
        if (ci.CompareTag("Source")) StopMining(ci.Col.GetComponent<ResourceSource>());
        else if (ci.CompareTag("Factory")) StopCraft();
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

    #region Mining

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
        BreakAnim();

        var animTimeMultiply = _mineStrikePeriod > DefaultMineStrikeTime ? 1 : (_mineStrikePeriod / DefaultMineStrikeTime * 0.9f);
        
        _animLTID =
            LeanTween.scaleY(visualTran.gameObject, 1.2f, DefaultMineStrikeTime * 0.8f * animTimeMultiply).setOnComplete(() =>
            {
                _animLTID =
                    LeanTween.scaleY(visualTran.gameObject, 1f, DefaultMineStrikeTime * 0.2f * animTimeMultiply).setOnComplete(() => _currentSource.TakeHit()).id;
            }).id;
    }
    
    private void BreakAnim()
    {
        if (_animLTID == -1) return;
        LeanTween.cancel(_animLTID);
        visualTran.localScale = Vector3.one;
        _animLTID = -1;
    }
    
    #endregion

    #region Craft

    public void StartCraft(Factory factory)
    {
        _isTransaction = true;
        _currentFactory = factory;
        
        if (_transactionCoroutine != null) StopCoroutine(_transactionCoroutine);
        _transactionCoroutine = StartCoroutine(CraftCor());
    }
    
    public void StopCraft()
    {
        _isTransaction = false;
        if (_transactionCoroutine != null) StopCoroutine(_transactionCoroutine);
        _currentFactory = null;
        
        RetryInteract();
    }
    
    private IEnumerator CraftCor()
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