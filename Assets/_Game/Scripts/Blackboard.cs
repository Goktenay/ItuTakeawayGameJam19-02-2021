using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Blackboard : MonoBehaviour
{
    #region Singleton

    private static Blackboard _instance;
    public static Blackboard Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion
    
    
    
    private PlayerController _playerController;
    private Transform _cameraTransform;

    private CursorController _cursorController;

    
    



    
    
    public PlayerController PlayerController
    {
        get
        {
            if (!_isPlayerControllerInitialized)
            {
                _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
                _isPlayerControllerInitialized = true;
            }

            return _playerController;
        }
    }

    public CursorController CursorController
    {
        get
        {
            if (!_isCursorControllerInitialized)
            {
                _cursorController = FindObjectOfType<CursorController>();
                _isCursorControllerInitialized = true;
            }

            return _cursorController;
        }
    }
    
    
    private bool _isCursorControllerInitialized = false;
    private bool _isPlayerControllerInitialized = false;

    
    
    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(0);
        }
    }


}
