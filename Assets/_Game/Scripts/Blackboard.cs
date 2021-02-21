using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
    [SerializeField] private GameObject _coolTextImageGameObject;
    [SerializeField] private TextMeshProUGUI _coolTextTMP;
    
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

    private bool _isShowingCoolText;
    private bool _coolTextPunchBool;
    private bool _coolTextSwingBool;
    private bool _coolTextWakeUpBool;
    private bool _coolTextSlowTimeBool;
    private bool _timeBetweenShowCoolTextPassed = true;
    
    public void OnPlayerAction(PlayerActionCool cool)
    {
        if(_isShowingCoolText)
            return;

     
        

        float randomVar = Random.Range(0f, 1f);
        switch (cool)
        {
            case PlayerActionCool.Falls:
                if (randomVar < 0.5)
                {
                    ShowCoolTextButImmediate("Commits Suicide");
                }
                else
                {
                    ShowCoolTextButImmediate("Falls");
                }
              
                break;
            case PlayerActionCool.GetHitsByABullet:
                if (randomVar < 0.5)
                {
                    ShowCoolTextButImmediate("Eats Bullet");
                }
                else
                {
                    ShowCoolTextButImmediate("Tragically Dies");
                }
                    break;
            case PlayerActionCool.Punches:
                if (!_coolTextPunchBool && _timeBetweenShowCoolTextPassed)
                {
                    ShowCoolText("Punches");
                    _coolTextPunchBool = true;
                }
                break;
            case PlayerActionCool.Swings:
                if (!_coolTextSwingBool && _timeBetweenShowCoolTextPassed)
                {
                    ShowCoolText("Swings");
                    _coolTextSwingBool = true;
                }
                break;
            case PlayerActionCool.WakesUp:
                if (!_coolTextWakeUpBool && _timeBetweenShowCoolTextPassed)
                {
                    ShowCoolText("Wakes Up");
                    _coolTextWakeUpBool = true;
                }
                break;
            case PlayerActionCool.SlowsTime:
                if (!_coolTextSlowTimeBool && _timeBetweenShowCoolTextPassed)
                {
                    ShowCoolText("Slows Time");
                    _coolTextSlowTimeBool = true;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(cool), cool, null);

        }

        

        void ShowCoolTextButImmediate(string text)
        {
            _isShowingCoolText = true;
            SetPlayTweens(false);
                float currentTimeScale = Time.timeScale;
                Time.timeScale = 0;
                _coolTextImageGameObject.SetActive(true);
                _coolTextTMP.text = "He "  + text;

                DOVirtual.DelayedCall(1f, () =>
                {
                    _coolTextImageGameObject.SetActive(false);
                    Time.timeScale = currentTimeScale;
                    SetPlayTweens(true);

                    _isShowingCoolText = false;
                });
                
            
            


            void SetPlayTweens(bool val)
            {
                if (val)
                {
                    if (_timeTween != null)
                    {
                    
                        _timeTween.Play();
                    }
        
                    if (_timeShaderTween != null)
                    {
                        _timeShaderTween.Play();
                    }
                }
                else
                {
                    if (_timeTween != null)
                    {
                    
                        _timeTween.Pause();
                    }
        
                    if (_timeShaderTween != null)
                    {
                        _timeShaderTween.Pause();
                    }
                }

            }
        }
        
        void ShowCoolText(string text)
        {
            _timeBetweenShowCoolTextPassed = false;

            DOVirtual.DelayedCall(10, () => _timeBetweenShowCoolTextPassed = true);
            
            _isShowingCoolText = true;
            DOVirtual.DelayedCall(2, () =>
            {
                SetPlayTweens(false);
                float currentTimeScale = Time.timeScale;
                Time.timeScale = 0;
                _coolTextImageGameObject.SetActive(true);
                _coolTextTMP.text = "He";

                DOVirtual.DelayedCall(0.5f, () =>
                {
                    _coolTextImageGameObject.SetActive(false);
                    Time.timeScale = currentTimeScale;
                    SetPlayTweens(true);

                    DOVirtual.DelayedCall(2, () =>
                    {
                        currentTimeScale = Time.timeScale;
                        _coolTextImageGameObject.SetActive(true);
                        _coolTextTMP.text = text;

                        SetPlayTweens(false);
                        DOVirtual.DelayedCall(0.5f, () =>
                        {
                            SetPlayTweens(true);
                            _coolTextImageGameObject.SetActive(false);
                            _isShowingCoolText = false;
                            Time.timeScale = currentTimeScale;
                        });
                    });

                });

            });
            

            


            void SetPlayTweens(bool val)
            {
                if (val)
                {
                    if (_timeTween != null)
                    {
                    
                        _timeTween.Play();
                    }
        
                    if (_timeShaderTween != null)
                    {
                        _timeShaderTween.Play();
                    }
                }
                else
                {
                    if (_timeTween != null)
                    {
                    
                        _timeTween.Pause();
                    }
        
                    if (_timeShaderTween != null)
                    {
                        _timeShaderTween.Pause();
                    }
                }

            }
            
        }
        
    }


}

public enum PlayerActionCool
{
    Falls, GetHitsByABullet, Punches, Swings,
    WakesUp, SlowsTime

}

