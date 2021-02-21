using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingHookableBehaviour : MonoBehaviour, IHookable
{

    [SerializeField] private Vector3 _movementVec;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _hookableDistance = 20;
    [SerializeField] private bool _doesStopWhenNotHooked = false;

    private Transform _tempTransform;
    private Vector3 _hitPos;
    private Vector3 _startPos;
    private void Start()
    {
        _startPos = transform.position;
        Blackboard.Instance.OnPlayerKilledEvent += OnPlayerDiedActions;
    }

    private void OnPlayerDiedActions()
    {
        _startMovement = false;
        _movementVec.Normalize();
        transform.position = _startPos;
    }

    private void FixedUpdate()
    {
        if (_startMovement)
        {
            _rigidbody.MovePosition(transform.position + Time.fixedDeltaTime * _movementSpeed * _movementVec);
        }
    }

    private void OnDestroy()
    {
        Blackboard.Instance.OnPlayerKilledEvent -= OnPlayerDiedActions;
        if (_tempTransform != null)
        {
            OnHookEnd(_tempTransform);
            Blackboard.Instance.PlayerController.OnAttachedHookableObjectDestroyed();
        }
    }

    private bool _startMovement;

    public bool TryToGetHookableCondition(RaycastHit info, Ray ray)
    {
        if (info.distance < _hookableDistance)
        {
            _hitPos = info.point;
            return true;
        }

        return false;
    }

    public void OnHookStart(Transform hookTransform)
    {
        hookTransform.position = _hitPos;
        hookTransform.SetParent(transform);
        _startMovement = true;
        _tempTransform = hookTransform;
    }

    public void OnHookUpdate(Transform hookTransform)
    {
     
    }

    public void OnHookEnd(Transform hookTransform)
    {
        if (_doesStopWhenNotHooked)
        {
            _startMovement = false;
        }
        
        hookTransform.SetParent(null);
        _tempTransform = null;
    }
}
