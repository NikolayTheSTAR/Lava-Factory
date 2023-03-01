using System;
using System.Collections;
using System.Collections.Generic;
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

    private bool _isMining = false;

    private ResourceSource _currentSource;
    private Coroutine _mineCoroutine;
    private int _animLTID = -1;

    public void Init()
    {
        trigger.Init(OnEnter, OnExit);
    }
    
    public void JoystickInput(Vector2 input)
    {
        Vector3 finalMoveDirection;

        if (input.x != 0 || input.y != 0)
        {
            var tempMoveDirection = new Vector3(input.x, 0, input.y);
            finalMoveDirection = transform.position + tempMoveDirection;
        }
        else finalMoveDirection = transform.position;

        meshAgent.SetDestination(finalMoveDirection);
    }

    private void OnEnter(Collider other)
    {
        if (!other.CompareTag("Source")) return;
        var r = other.GetComponent<ResourceSource>();
        if (r == null) return;
        StartMining(r);
    }
    
    private void OnExit(Collider other)
    {
        if (!other.CompareTag("Source")) return;
        var r = other.GetComponent<ResourceSource>();
        if (r == null) return;
        StopMining();
    }

    #region Minig

    private void StartMining(ResourceSource source)
    {
        _isMining = true;
        _currentSource = source;
        _mineCoroutine = StartCoroutine(MiningCor());
    }
        
    private void StopMining()
    {
        _isMining = false;
        LeanTween.cancel(_animLTID);
        StopCoroutine(_mineCoroutine);

        _animLTID =
            LeanTween.scaleY(gameObject, 1, DefaultMineStrikeTime).id;
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

    #endregion
}