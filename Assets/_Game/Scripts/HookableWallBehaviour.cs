using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookableWallBehaviour : MonoBehaviour, IHookable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void OnHookStart()
    {
    }

    public void OnHookUpdate()
    {
    }

    public void OnHookEnd()
    {
    }
    
    
    private Transform _tempTransform;
    private Vector3 _hookOffsetPoint;

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
        
        _hookOffsetPoint = info.point;
        return true;
    }

    public void OnHookStart(Transform hookTransform)
    {
        hookTransform.position = _hookOffsetPoint;
        hookTransform.SetParent(transform);
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
