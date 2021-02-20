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

    private void Start()
    {
        _image.color = _defaultColor;
    }

    public void OnReadyToHookStateChange(bool val)
    {
        if (val)
        {
            _image.color = _hookableColor;
        }
        else
        {
            _image.color = _defaultColor;
        }
        
    }
}
