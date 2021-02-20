using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    [Header("Dependencies")] 
    [SerializeField] private float _globalTimeMultiplier;

    private PlayerController _playerController;
    private Transform _cameraTransform;
    public float GlobalTimeMultiplier => _globalTimeMultiplier;


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


    private bool _isPlayerControllerInitialized = false;

    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
