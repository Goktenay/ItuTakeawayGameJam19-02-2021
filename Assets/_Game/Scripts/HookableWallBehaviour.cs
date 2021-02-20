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

    public HookableMetaData TryToGetHookableCondition(RaycastHit info)
    {
        HookableMetaData data = new HookableMetaData();
        data.Hookable = this;
        data.CanHook = true;
        data.OffsetPosition= info.point - transform .position;
        data.TransformToFollow = transform;
        return data;
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
}
