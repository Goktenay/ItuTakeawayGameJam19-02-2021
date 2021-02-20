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
    
    
    // Start is called before the first frame update
    void Start()
    {
        
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

    public HookableMetaData TryToGetHookableCondition(RaycastHit info, Transform hookableTempTransform)
    {
        HookableMetaData data = new HookableMetaData();
        data.Hookable = this;
        hookableTempTransform.SetParent(transform);
        hookableTempTransform.position = transform.position;
        data.TransformToFollow = hookableTempTransform;
        
        
        data.CanHook = true; 
        
        return  data;
    }

    public void OnHookStart()
    {
        _currentSpeed = 0;
    }

    public void OnHookUpdate()
    {
        _aliveTime -= Time.fixedDeltaTime ;
    }

    public void OnHookEnd()
    {
        _currentSpeed = _speed;
    }
}
