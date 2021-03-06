﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour , IHookable
{
    [Header("Dependencies")] 
    [SerializeField] private Rigidbody _rigidbody;
    
    
    [Header("Settings")] 
    [SerializeField] private float _speed;
    [SerializeField] private float _lifespan;
    [SerializeField] private bool _isHookable;
    [SerializeField] private bool _doesStopWhenNotHooked = false;
    [SerializeField] private float _hookableDistance = 1000;
    private float _currentSpeed;

    private Vector3 _direction;
    private float _aliveTime;
    
    public void Initialize(Vector3 direction)
    {
        _currentSpeed = _speed;
        _direction = direction.normalized;
        _aliveTime = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void InitializeWithSpeed(Vector3 dir, float speed)
    {
        _speed = speed;
        _currentSpeed = _speed;
        _direction = dir.normalized;
        _aliveTime = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        Blackboard.Instance.OnPlayerKilledEvent += OnPlayerKilledActions;
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + Time.fixedDeltaTime * _currentSpeed * _direction);

        _aliveTime += Time.fixedDeltaTime;

        if (_aliveTime > _lifespan)
        {
            Destroy(gameObject);
        }
    }

    
    private Transform _tempTransform;

    private void OnDestroy()
    {
        Blackboard.Instance.OnPlayerKilledEvent -= OnPlayerKilledActions;
        
        if (_tempTransform != null)
        {
            OnHookEnd(_tempTransform);
            Blackboard.Instance.PlayerController.OnAttachedHookableObjectDestroyed();
        }
    }


    public bool TryToGetHookableCondition(RaycastHit info, Ray ray)
    {
        if (_isHookable && info.distance < _hookableDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public void OnHookStart(Transform hookTransform)
    {
        hookTransform.SetParent(transform);
        hookTransform.localPosition = Vector3.zero;
        _tempTransform = hookTransform;

        if (_doesStopWhenNotHooked)
        {
            _currentSpeed = 0;
        }
        
       // _currentSpeed = 0;
    }

    public void OnHookUpdate(Transform hookTransform)
    {
       // _aliveTime -= Time.fixedDeltaTime ;
    }

    public void OnHookEnd(Transform hookTransform)
    {
        if (_doesStopWhenNotHooked)
        {
            _currentSpeed = _speed;
        }
        
       // _currentSpeed = _speed;
        hookTransform.SetParent(null);
        _tempTransform = null;
    }

    private void OnPlayerKilledActions()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Surface"))
        {
            Destroy(gameObject);
        }
    }
}
