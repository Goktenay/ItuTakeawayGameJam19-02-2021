using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticHookableBehaviour : MonoBehaviour, IHookable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private Transform _tempTransform;

    private void OnDestroy()
    {
        if (_tempTransform != null)
        {
            OnHookEnd(_tempTransform);
            Blackboard.Instance.PlayerController.OnAttachedHookableObjectDestroyed();
        }
    }


    public bool TryToGetHookableCondition(RaycastHit info)
    {
        return true;
    }

    public void OnHookStart(Transform hookTransform)
    {
        hookTransform.SetParent(transform);
        hookTransform.localPosition = Vector3.zero;
        _tempTransform = hookTransform;
    }

    public void OnHookUpdate(Transform hookTransform)
    {
     
    }

    public void OnHookEnd(Transform hookTransform)
    {
        hookTransform.SetParent(null);
        _tempTransform = null;
    }
}
