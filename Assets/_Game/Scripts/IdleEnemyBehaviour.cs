using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IdleEnemyBehaviour : MonoBehaviour , IHookable
{
    [Header("Dependencies")] 
    [SerializeField] private Transform _rotationPivotTransform;
    [SerializeField] private Transform _bulletSpawnTransform; 
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Rigidbody _rigidbody;
    
    [Header("Settings")] 
    [SerializeField] private float _rotateTowardsPlayerLerpSpeed;
    [SerializeField] private float _bulletSpawnCooldown;
    [SerializeField] private float _playerStartShootingTriggerRadius;
    [SerializeField] private float _playerStopShootingTriggerRadius = 6;
    [SerializeField] private float _hookableDistance = 5;
    
    private float _timer;
    private bool _isDefeated;
    
    
    // Start is called before the first frame update
    void Start()
    {
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
                _rotationPivotTransform.rotation =
                    Quaternion.Lerp(_rotationPivotTransform.rotation,
                        Quaternion.LookRotation(transformToPlayer),
                        Time.deltaTime * _rotateTowardsPlayerLerpSpeed );

                if (_timer > _bulletSpawnCooldown)
                {
                    _timer = 0;
                    SpawnBullet();
                }
                
            }


            _timer += Time.deltaTime;
        }
    }

    private void SpawnBullet()
    {
        GameObject bullet = Instantiate(_bulletPrefab, _bulletSpawnTransform.position, Quaternion.identity);
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

}
