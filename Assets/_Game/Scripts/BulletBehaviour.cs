using System;
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


    private Vector3 _direction;
    private float _aliveTime;
    
    public void Initialize(Vector3 direction)
    {
        _direction = direction.normalized;
        _aliveTime = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + Blackboard.Instance.GlobalTimeMultiplier* Time.fixedDeltaTime * _speed * _direction);

        _aliveTime += Time.fixedDeltaTime * Blackboard.Instance.GlobalTimeMultiplier;

        if (_aliveTime > _lifespan)
        {
            Destroy(gameObject);
        }
    }

    public HookableMetaData TryToHook()
    {
        return  new HookableMetaData();
    }
}
