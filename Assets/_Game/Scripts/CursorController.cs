using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    [Header("Dependencies")] 
    [SerializeField] private Image _image;
    
    [Header("Settings")] 
    [SerializeField] private Color _hookableColor;

    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _hookedColor;

    private void Start()
    {
        _image.color = _defaultColor;
    }

    public void OnReadyToHookStateChange(HookCursorEnum val)
    {
        switch (val)
        {
            case HookCursorEnum.ReadyToHook:
                _image.color = _hookableColor;
                break;
            case HookCursorEnum.Hooked:
                _image.color = _hookedColor;
                break;
            case HookCursorEnum.NotHookable:
                _image.color = _defaultColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(val), val, null);
        }
        

        
    }
    
}


public enum HookCursorEnum
{
    ReadyToHook, Hooked, NotHookable
}
