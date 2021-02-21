﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossEnemyBehaviour : MonoBehaviour , IHookable
{
    [Header("Dependencies")] 
    [SerializeField] private Transform _bulletSpawnTransform; 
    [SerializeField] private GameObject _nonHookablebulletPrefab;
    [SerializeField] private GameObject _hookableBulletPrefab;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Vector3 _spawnDirection;
    
    [Header("State 0")] 
    [SerializeField] private float _state0BulletSpawnCooldown;
    [SerializeField] private float _state0BulletSpeed;
    
    
    
    
    
    [Header("Settings")]
    [SerializeField] private float _playerStartShootingTriggerRadius;
    [SerializeField] private float _playerStopShootingTriggerRadius = 6;
    [SerializeField] private float _hookableDistance = 5;
    
    private float _timer;
    private bool _isDefeated;


    private StateEnum _stateEnum;
    
    // Start is called before the first frame update
    void Start()
    {
        _stateEnum = StateEnum.State0;
        Blackboard.Instance.OnPlayerKilledEvent += OnPlayerIsDead;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isDefeated)
        {
            
            Vector3 transformToPlayer = Blackboard.Instance.PlayerController.BulletAimTransform.position - transform.position;
            float playerSqrMag = transformToPlayer.sqrMagnitude;
            
            if (playerSqrMag < _playerStartShootingTriggerRadius * _playerStartShootingTriggerRadius && playerSqrMag > _playerStopShootingTriggerRadius * _playerStopShootingTriggerRadius)
            {

                if (_stateEnum == StateEnum.State0)
                {
                    State0Actions();
                }
                else if (_stateEnum == StateEnum.State1)
                {
                    State1Actions();
                }
                else if (_stateEnum == StateEnum.State2)
                {
                    State2Actions();
                }

            }
          
        }
    }

    private int _state0Test = 0;
    
    private void State0Actions()
    {
        if (_timer > _state0BulletSpawnCooldown)
        {
            _state0Test++;
            Vector3 posRelative =   _bulletSpawnTransform.position;
            Vector3 rightVec = new Vector3(-_spawnDirection.z, 0, _spawnDirection.x);
            int bulletCount = 20;
            int spawnDistance = 4;
            for (int i = 0; i < bulletCount; i++)
            {
                Vector3 spawnPos = posRelative + spawnDistance * (i - bulletCount/2f) * rightVec ;
                spawnPos += Random.Range(-1f, 1f) * Vector3.up * 15;
            
                GameObject bullet = Instantiate(i % 2 == 0 ? _hookableBulletPrefab : _nonHookablebulletPrefab, spawnPos,
                    Quaternion.identity);

                BulletBehaviour behaviour = bullet.GetComponent<BulletBehaviour>();
                behaviour.InitializeWithSpeed(_spawnDirection, _state0BulletSpeed);

            }

            _timer = 0;
        }

        
        
        
        
        _timer += Time.deltaTime;
    }
    
    private void State1Actions()
    {
        
    }

    
    private void State2Actions()
    {
        
    }

    

    private void SpawnBullet()
    {
        GameObject bullet = Instantiate(_nonHookablebulletPrefab, _bulletSpawnTransform.position, Quaternion.identity);
        bullet.GetComponent<BulletBehaviour>().Initialize(_bulletSpawnTransform.forward);
    }

    private void OnDrawGizmos()
    {

        DrawCircleAroundActor(_playerStartShootingTriggerRadius, Color.red);
        DrawCircleAroundActor(_playerStopShootingTriggerRadius, Color.green);
        
        void DrawCircleAroundActor(float radiusMult, Color col)
        {
            Vector3 firstLinePos = new Vector3(1, 0, 0) * radiusMult + transform.position;
            Vector3 lastLinePos = firstLinePos;
            Gizmos.color = col;
            for (int i = 0; i < 16; i++)
            {
                float rad = (Mathf.PI * i) / 8;
                Vector3 offsetVec = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radiusMult;
                Vector3 currentLinePos = offsetVec + transform.position;
                Gizmos.DrawLine(lastLinePos, currentLinePos);
                lastLinePos = currentLinePos;

                if (i == 15)
                {
                    Gizmos.DrawLine(lastLinePos, firstLinePos);
                }
            }
        }

    }

    private Transform _tempTransform;
    private Ray _playerHitRay;
    
    private void OnDestroy()
    {
        Blackboard.Instance.OnPlayerKilledEvent -= OnPlayerIsDead;

        if (_tempTransform != null)
        {
            OnHookEnd(_tempTransform);
            Blackboard.Instance.PlayerController.OnAttachedHookableObjectDestroyed();
        }
    }


    public bool TryToGetHookableCondition(RaycastHit info, Ray ray)
    {
        if (info.distance < _hookableDistance && !_isDefeated)
        {
            _playerHitRay = ray;
            return true;
        }

        return false;
    }

    public void OnHookStart(Transform hookTransform)
    {
        hookTransform.SetParent(transform);
        hookTransform.localPosition = Vector3.zero;
        _tempTransform = hookTransform;
        

    }

    public void OnHookUpdate(Transform hookTransform)
    {
        if (!_isDefeated)
        {
            Blackboard.Instance.OnPlayerAction(PlayerActionCool.Punches);
            _rigidbody.isKinematic = false;
            _isDefeated = true;
            Blackboard.Instance.PlayerController.OnAttachedHookableObjectDestroyed();
            _rigidbody.AddForce(_playerHitRay.direction * 1000, ForceMode.Force);
            _rigidbody.AddTorque(Random.insideUnitSphere * 1000, ForceMode.Force);
        }

    }

    public void OnHookEnd(Transform hookTransform)
    {
        hookTransform.SetParent(null);
        _tempTransform = null;
    }

    private void OnPlayerIsDead()
    {
        _timer = -2;
    }

    enum StateEnum
    {
       State0, State1, State2 
    }
    
}
