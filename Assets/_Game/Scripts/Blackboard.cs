using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [Header("Settings")] 
    [SerializeField] private Color _hookableColor;
    [SerializeField] private Color _nonHookableBulletColor;
    [SerializeField] private Color _hookableBulletColor;
    [SerializeField] private Color _enemySpawnerColor;

    public delegate void OnPlayerKilledDelegate();
    public event OnPlayerKilledDelegate OnPlayerKilledEvent;
    
    
    private PlayerController _playerController;
    private Transform _cameraTransform;

    private CursorController _cursorController;

    

    private Tween _timeTween;
    private Tween _timeShaderTween;
    private int _SlowTimeGlobalValueShaderId;
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Hide and lock cursor when right mouse button pressed
        

        // Unlock and show cursor when right mouse button released
  
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        
        _SlowTimeGlobalValueShaderId = Shader.PropertyToID("_SlowTimeGlobalValue");
        Shader.SetGlobalColor("_HookableColor", _hookableColor);
        Shader.SetGlobalColor("_NonHookableBulletColor", _nonHookableBulletColor);
        Shader.SetGlobalColor("_HookableBulletColor", _hookableBulletColor);
     
        Shader.SetGlobalColor("_EnemySpawnerColor", _enemySpawnerColor);
        
    }

    
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

    
    

    // Update is called once per frame
    void Update()
    {
  
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(0);
        }
    }


    public void SlowTime()
    {
        if (_timeTween != null)
        {
            _timeTween.Kill();
        }

        if (_timeShaderTween != null)
        {
            _timeShaderTween.Kill();
        }
        

        _timeShaderTween = DOVirtual.Float(Shader.GetGlobalFloat(_SlowTimeGlobalValueShaderId), 1f, 0.1f, value =>
            {
                Shader.SetGlobalFloat(_SlowTimeGlobalValueShaderId,value );
            }).SetUpdate(UpdateType.Late)
            .SetEase(Ease.InSine);
        
        _timeTween = DOVirtual.Float(Time.timeScale, 0.1f, 0.2f, value =>
        {
            Time.timeScale = value;
            Time.fixedDeltaTime = 0.02f * value;
        }).SetUpdate(UpdateType.Late).SetEase(Ease.OutSine);

    }

    public void SpeedTime()
    {
        if (_timeTween != null)
        {
            _timeTween.Kill();
        }
        
        if (_timeShaderTween != null)
        {
            _timeShaderTween.Kill();
        }
        
        _timeShaderTween = DOVirtual.Float(Shader.GetGlobalFloat(_SlowTimeGlobalValueShaderId), 0f, 0.1f, value =>
            {
                Shader.SetGlobalFloat(_SlowTimeGlobalValueShaderId,value );
            }).SetUpdate(UpdateType.Late)
            .SetEase(Ease.InSine);
        
        _timeTween = DOVirtual.Float(Time.timeScale, 1f, 0.1f, value =>
            {
         
                Time.timeScale = value;
                Time.fixedDeltaTime = 0.02f * value;
            }).SetUpdate(UpdateType.Late)
            .SetEase(Ease.InSine);
    }

    public void OnPlayerKilled()
    {
        OnPlayerKilledEvent?.Invoke();

    }
    
    
}
