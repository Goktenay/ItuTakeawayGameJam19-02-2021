using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHookable
{
    HookableMetaData TryToGetHookableCondition(RaycastHit info);
    
    void OnHookStart();
    void OnHookUpdate();
    void OnHookEnd();

    
}

public class HookableMetaData
{
    public IHookable Hookable;
    public bool CanHook;
    public Vector3 OffsetPosition;
    public Transform TransformToFollow;

    public Vector3 GetPosition()
    {
        Vector3 pos = OffsetPosition;

        if (TransformToFollow != null)
        {
            pos += TransformToFollow.position;
        }

        return pos;

    }

}


