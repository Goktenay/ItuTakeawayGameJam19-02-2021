using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHookable
{
    
    bool TryToGetHookableCondition(RaycastHit info);
    
    void OnHookStart(Transform hookTransform);
    void OnHookUpdate(Transform hookTransform);
    void OnHookEnd(Transform hookTransform);

    
}

