using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticHookableBehaviour : MonoBehaviour, IHookable
{
    
    [SerializeField] private float _hookableDistance = 20;


    private Transform _tempTransform;

    private void OnDestroy()
    {
        if (_tempTransform != null)
        {
            OnHookEnd(_tempTransform);
            Blackboard.Instance.PlayerController.OnAttachedHookableObjectDestroyed();
        }
    }


    public bool TryToGetHookableCondition(RaycastHit info, Ray ray)
    {
        if (info.distance < _hookableDistance)
        {
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
     
    }

    public void OnHookEnd(Transform hookTransform)
    {
        hookTransform.SetParent(null);
        _tempTransform = null;
    }
}
