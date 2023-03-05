using System;
using System.Collections;
using System.Collections.Generic;
using Configs;
using Mining;
using TheSTAR.Input;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using World;

public class Player : MonoBehaviour, ICameraFocusable, IJoystickControlled, IDropReceiver
{
    [SerializeField] private NavMeshAgent meshAgent;
    [SerializeField] private EntranceTrigger trigger;
    [SerializeField] private float mineStrikePeriod = 1;

    private const float DefaultMineStrikeTime = 0.5f;

    private bool _isMoving = false;
    private bool _isMining = false;
    
    private ICollisionInteractable _currentCollisionInteractable;
    private ResourceSource _currentSource;
    private Coroutine _mineCoroutine;
    private int _animLTID = -1;

    public delegate void OnStartMiningDelegate(SourceType sourceType, out SourceMiningData miningData);

    private OnStartMiningDelegate _onStartMining;

    public void Init(OnStartMiningDelegate onStartMining)
    {
        trigger.Init(OnEnter, OnExit);
        _onStartMining = onStartMining;
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
        
        if (ci.Condition == ICICondition.None) ci.StopInteract(this);
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
        mineStrikePeriod = miningData.MiningPeriod;
        
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
            yield return new WaitForSeconds(mineStrikePeriod);
        }
        yield return null;
    }

    private void DoMineStrike()
    {
        if (_animLTID != -1) LeanTween.cancel(_animLTID);

        var animTimeMultiply = mineStrikePeriod > DefaultMineStrikeTime ? 1 : (mineStrikePeriod / DefaultMineStrikeTime * 0.9f);
        
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
}