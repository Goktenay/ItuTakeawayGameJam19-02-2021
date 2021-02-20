using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHookable
{
    
    HookableMetaData TryToGetHookableCondition(RaycastHit info, Transform hookableTempTransform);
    
    void OnHookStart();
    void OnHookUpdate();
    void OnHookEnd();

    
}

public class HookableMetaData
{
    public IHookable Hookable;
    public bool CanHook;

    public Transform TransformToFollow;

    private int _registerCount;
    

    
}


