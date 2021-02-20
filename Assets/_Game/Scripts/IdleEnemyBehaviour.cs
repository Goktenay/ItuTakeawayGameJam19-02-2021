using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleEnemyBehaviour : MonoBehaviour
{
    [Header("Dependencies")] 
    [SerializeField] private Transform _rotationPivotTransform;
    [SerializeField] private Transform _bulletSpawnTransform; 
    [SerializeField] private GameObject _bulletPrefab;

    [Header("Settings")] 
    [SerializeField] private float _rotateTowardsPlayerLerpSpeed;
    [SerializeField] private float _bulletSpawnCooldown;
    [SerializeField] private float _playerStartShootingTriggerRadius;
    
    private float _timer;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 transformToPlayer = Blackboard.Instance.PlayerController.BulletAimTransform.position - transform.position;
        
        if (transformToPlayer.sqrMagnitude < _playerStartShootingTriggerRadius * _playerStartShootingTriggerRadius)
        {
            _rotationPivotTransform.rotation =
                Quaternion.Lerp(_rotationPivotTransform.rotation,
                    Quaternion.LookRotation(transformToPlayer),
                    Time.deltaTime * _rotateTowardsPlayerLerpSpeed * Blackboard.Instance.GlobalTimeMultiplier);

            if (_timer > _bulletSpawnCooldown)
            {
                _timer = 0;
                SpawnBullet();
            }
            
        }


        _timer += Time.deltaTime * Blackboard.Instance.GlobalTimeMultiplier;
    }

    private void SpawnBullet()
    {
        GameObject bullet = Instantiate(_bulletPrefab, _bulletSpawnTransform.position, Quaternion.identity);
        bullet.GetComponent<BulletBehaviour>().Initialize(_bulletSpawnTransform.forward);
    }

    private void OnDrawGizmos()
    {

        DrawCircleAroundActor(_playerStartShootingTriggerRadius, Color.green);
        
        
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
}
