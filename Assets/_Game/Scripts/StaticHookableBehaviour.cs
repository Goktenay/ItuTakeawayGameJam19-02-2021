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
    }

    public void OnHookUpdate()
    {
    }

    public void OnHookEnd()
    {
    }
}
