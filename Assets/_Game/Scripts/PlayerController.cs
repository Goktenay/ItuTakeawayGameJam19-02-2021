using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SEP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTemplateProjects;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")] 
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Transform _cameraPositionRefTransform;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private LayerMask _hookRaycastLayerMask;
    [SerializeField] private Transform _groundedRayCastTransform;
    [SerializeField] private Transform _bulletAimTransform;
    
 
    
    [Header("Settings")] 
    [SerializeField] private float _onGroundedMovementSpeed = 8;
    [SerializeField] private float _jumpSpeed;
    [Space(10)]
    [SerializeField] private float _upwardsGravity;
    [SerializeField] private float _downwardsGravity;
    [Space(10)]
    [SerializeField] private float _tetherTentionForce = 8;
    [SerializeField] private float _maxSwingInputForce;
    [SerializeField] private float _swingingFriction = 0.05f;
    [SerializeField] private float _swingingMaxSpeed;
    [Space(10)] 
    [SerializeField] private float _onAirMovementForce = 8;
    [SerializeField] private float _isOnAirAfterSwingingxzFriction = 0.05f;
    [Space(10)] 
    [SerializeField] private float _hookableDistance = 20;
    
    private Vector2Int _inputDirection = Vector2Int.zero;
    private float _currentGravity;
    private bool _isGrounded;
    private bool _isSwinging;
    private bool _isOnAirAfterSwinging = false;
    private bool _isOnGroundedAfterSwinging = true;
    private bool _isReadyToHook;
    private float _maxTetherLength;
    
    private InputFlag _jumpInput = new InputFlag(KeyCode.Space);
    private InputFlag _leftInput = new InputFlag(KeyCode.LeftArrow, KeyCode.A);
    private InputFlag _rightInput = new InputFlag(KeyCode.RightArrow, KeyCode.D);
    private InputFlag _backInput = new InputFlag(KeyCode.DownArrow, KeyCode.S);
    private InputFlag _forwardInput = new InputFlag(KeyCode.UpArrow, KeyCode.W);
    private InputFlag _mouse0Input = new InputFlag(KeyCode.Mouse0);
    private InputFlag _mouse1Input = new InputFlag(KeyCode.Mouse1);
    private InputFlag _qInput = new InputFlag(KeyCode.Q);

    public Rigidbody Rigidbody => _rigidbody;
    public Transform BulletAimTransform => _bulletAimTransform;

    private HookableMetaData _hookableMeta;
    
    // Start is called before the first frame update
    void Start()
    {
                
    }

    private void FixedUpdate()
    {
        MovementCalculations();
        ResetInputStartEndFlags();

        
        
        //////////////////////////////////
        void MovementCalculations()
        {
            CalculateInputDirection();
            CalculateOnGroundedVelocity();
            CalculateOnGrounded();
            CalculateJump();
            CalculateGravity();
            CalculateIsReadyToHook();
            CalculateSwing();
          

            
            
            
            /////////////////////////////////
            void CalculateInputDirection()
            {
                int horizontalInputDirection = _inputDirection.x;
                int verticalInputDirection = _inputDirection.y;
               
                
                if (_leftInput.Update && _rightInput.Update)
                {
                    if (_leftInput.Start)
                        horizontalInputDirection = -1;

                    if (_rightInput.Start)
                        horizontalInputDirection =  1;
                }
                else if (_leftInput.Update)
                {
                    horizontalInputDirection = -1;
                }
                else if (_rightInput.Update)
                {
                    horizontalInputDirection = 1;
                }
                else
                {
                    horizontalInputDirection = 0;
                }

                
                if (_backInput.Update && _forwardInput.Update)
                {
                    if (_backInput.Start)
                        verticalInputDirection = -1;

                    if (_forwardInput.Start)
                        verticalInputDirection =  1;
                }
                else if (_backInput.Update)
                {
                    verticalInputDirection = -1;
                }
                else if (_forwardInput.Update)
                {
                    verticalInputDirection = 1;
                }
                else
                {
                    verticalInputDirection = 0;
                }
            

                _inputDirection = new Vector2Int(horizontalInputDirection, verticalInputDirection);
            }
            
            void CalculateOnGroundedVelocity()
            {
                if (_isOnGroundedAfterSwinging && !_isSwinging)
                {
                    float yawValue =_cameraController.CameraYawValue * Mathf.Deg2Rad;
                    Vector2 forwardVec = new Vector2(Mathf.Sin(yawValue), Mathf.Cos(yawValue));
                    forwardVec.Normalize();
                    Vector2 rightVec = new Vector2(forwardVec.y, -forwardVec.x);

                
                    Vector2 horizontalVel = _inputDirection.x * _onGroundedMovementSpeed * rightVec;
                    Vector2 verticalVel = _inputDirection.y * _onGroundedMovementSpeed * forwardVec;
                    Vector2 xzSum = Vector2.ClampMagnitude(horizontalVel + verticalVel, _onGroundedMovementSpeed);
                    
                    Vector3 vel = _rigidbody.velocity;
                    vel.x = xzSum.x;
                    vel.z = xzSum.y;
                    _rigidbody.velocity = vel;
                    
                }


 
                
                //Debug.DrawRay(transform.position, new Vector3(forwardVec.x, 0, forwardVec.y));
            }

            void CalculateGravity()
            {
                if (_rigidbody.velocity.y > 0 && _jumpInput.Update)
                {
                    _currentGravity = _upwardsGravity;
                }
                else
                {
                    _currentGravity = _downwardsGravity;
                }
                
                
                _rigidbody.AddForce( _currentGravity * Vector3.up, ForceMode.Acceleration);
            }


            void CalculateJump()
            {
                if (_isGrounded && _jumpInput.Start)
                {
                    Vector3 vel = _rigidbody.velocity;
                    vel.y = _jumpSpeed;
                    _rigidbody.velocity = vel;
                    _isGrounded = false;
                }
            }
            
            void CalculateOnGrounded()
            {
                Ray ray = new Ray(_groundedRayCastTransform.position, Vector3.down);
                
               // Debug.DrawRay(ray.origin, ray.direction * 0.5f, Color.red, 3);
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.5f, _groundLayerMask))
                {
                    if (_rigidbody.velocity.y <= 0)
                    {
                        _isGrounded = true;

                        if (!_isSwinging)
                        {
                            _isOnGroundedAfterSwinging = true; 
                            _isOnAirAfterSwinging = false;
                        }
                    }
                    else
                    {
                        _isGrounded = false;
                    }
                }
                else
                {
                    _isGrounded = false;
                }
            }

            void CalculateIsReadyToHook()
            {
                bool isReadyToHookLocal = _isReadyToHook; 
                
                _isReadyToHook = false;
                if (!_isSwinging)
                {
                    Ray ray = new Ray(_cameraController.transform.position, _cameraController.ForwardVector);

                    if (Physics.Raycast(ray, out RaycastHit hitInfo, _hookableDistance, _hookRaycastLayerMask))
                    {
                        IHookable hookable = hitInfo.collider.GetComponentInParent<IHookable>();

                        if (hookable != null)
                        {
                            HookableMetaData data = hookable.TryToGetHookableCondition(hitInfo);
                            if (data.CanHook)
                            {
                                _isReadyToHook = true;
                                
                                _hookableMeta = data;
                              //  Debug.Log("I CAN HOOOK");
                            }
                        }
                        else
                        {
                            Debug.LogError("ERROR this shouldnt happen -> " + hitInfo.collider.gameObject.name);
                        }
                    }
                }

                if (isReadyToHookLocal != _isReadyToHook)
                {
                    // state Change
                    Blackboard.Instance.CursorController.OnReadyToHookStateChange(_isReadyToHook);
                    
                }
                
                
            }
            
            void CalculateSwing()
            {

                if (!_isSwinging && _isReadyToHook)
                {
                    if (_mouse0Input.Start)
                    {
                        SwingInputStartActions();
                     
                    }
                }

                if (_isSwinging)
                {
                    if (_mouse0Input.Update)
                    {
                        SwingInputUpdateActions();
                    }


                    if (_mouse0Input.End)
                    {
                        SwingInputEndActions();
                    }
                }
                
                
                void SwingInputStartActions()
                {
                    _hookableMeta.Hookable.OnHookStart();
                    _maxTetherLength = (_rigidbody.position - _hookableMeta.GetPosition()).magnitude;
                    _isOnAirAfterSwinging = false;
                    _isSwinging = true;
                }

                void SwingInputUpdateActions()
                {
                    _hookableMeta.Hookable.OnHookUpdate();
                     _isOnGroundedAfterSwinging = false;
                     
                    Debug.DrawLine(_rigidbody.position, _hookableMeta.GetPosition(), Color.green);
                    Debug.DrawRay(_rigidbody.position, _rigidbody.velocity, Color.red);
                    Vector3 forwardVec = _cameraController.ForwardVector;
                    Vector3 rightVec = _cameraController.RightVector;

                    rightVec *= _maxSwingInputForce * _inputDirection.x;
                    forwardVec *= _maxSwingInputForce * _inputDirection.y;
                    Vector3 sumVec = rightVec + forwardVec;
                    sumVec = Vector3.ClampMagnitude(sumVec, _maxSwingInputForce);
                    
                    _rigidbody.AddForce(sumVec, ForceMode.Acceleration);

                    if (_isGrounded)
                    {
                        _rigidbody.AddForce( (_hookableMeta.GetPosition() - _rigidbody.position).normalized * _tetherTentionForce, ForceMode.Acceleration );
                    }
                    
                    Vector3 newPos = _rigidbody.position + _rigidbody.velocity * Time.fixedDeltaTime;
                    Vector3 tetherToNewPos = (newPos - _hookableMeta.GetPosition());
                    tetherToNewPos = Vector3.ClampMagnitude(tetherToNewPos, _maxTetherLength);

                    if (tetherToNewPos.magnitude < _maxTetherLength)
                    {
                        _maxTetherLength = tetherToNewPos.magnitude;
                    }
                    
                    
                    _rigidbody.velocity = (tetherToNewPos - _rigidbody.position + _hookableMeta.GetPosition()).normalized *  _rigidbody.velocity.magnitude;

                    _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _swingingMaxSpeed);
                    _rigidbody.velocity -= (Time.fixedDeltaTime * _swingingFriction * _rigidbody.velocity);
                        
                }
                
                void SwingInputEndActions()
                {
                    _hookableMeta.Hookable.OnHookEnd();
                    _isSwinging = false;
                    _isOnAirAfterSwinging = true;
                }
                
                
                
                

                if (_isOnAirAfterSwinging)
                {
                    
                    Vector3 forwardVec = _cameraController.ForwardVector;
                    Vector3 rightVec = _cameraController.RightVector;

                    rightVec *= _onAirMovementForce * _inputDirection.x;
                    forwardVec *= _onAirMovementForce * _inputDirection.y;
                    Vector3 sumVec = rightVec + forwardVec;
                    sumVec = Vector3.ClampMagnitude(sumVec, _onAirMovementForce);
                    _rigidbody.AddForce(sumVec);
                    
                    Vector3 vel = _rigidbody.velocity;
                    vel.x -= (vel.x * _isOnAirAfterSwingingxzFriction * Time.fixedDeltaTime);
                    vel.z -= (vel.z * _isOnAirAfterSwingingxzFriction * Time.fixedDeltaTime);
                    _rigidbody.velocity = vel;
                }
                
            }

            
        }
        
        void ResetInputStartEndFlags()
        {
            _jumpInput.ResetStartEndFlags(); 
            _leftInput.ResetStartEndFlags();
            _rightInput.ResetStartEndFlags();
            _forwardInput.ResetStartEndFlags();
            _backInput.ResetStartEndFlags();
            _mouse0Input.ResetStartEndFlags();
            _mouse1Input.ResetStartEndFlags();
            _qInput.ResetStartEndFlags();
        }
    }


    // Update is called once per frame
    void Update()
    {
        SetInputFlags();
        CalculateTimeSlowDown();
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        
        void CalculateTimeSlowDown()
        {
            if (_qInput.Start)
            {
                DOTween.Kill("SlowDownTween");
                
                DOVirtual.Float(Time.timeScale, 0.1f, 0.2f, value =>
                {
                    // Blackboard.Instance.GlobalTimeMultiplier = value;
                    Time.timeScale = value;
                    Time.fixedDeltaTime = 0.02f * value;
                    Debug.Log("In - " +value);
                }).SetEase(Ease.OutSine).SetId("SlowDownTween");
                    
                 
            }

            if (_qInput.End)
            {
                DOTween.Kill("SlowDownTween");
                DOVirtual.Float(Time.timeScale, 1f, 0.1f, value =>
                {
                    Debug.Log("Out - " +value);
                    Time.timeScale = value;
                    Time.fixedDeltaTime = 0.02f * value;
                }).SetUpdate(UpdateType.Normal)
                    .SetEase(Ease.InSine).SetId("SlowDownTween");
            }
                
            
            _qInput.ResetStartEndFlags();
        }
        
        
        void SetInputFlags()
        {
            ResetInputUpdateFlags();
            SetInputUpdateFlags();
            
            void SetInputUpdateFlags()
            {
                _forwardInput.SetFlags();
                _backInput.SetFlags();
                _leftInput.SetFlags();
                _rightInput.SetFlags();
                _jumpInput.SetFlags();
                _mouse0Input.SetFlags();
                _mouse1Input.SetFlags();
                _qInput.SetFlags();

            }
            void ResetInputUpdateFlags()
            {
                _forwardInput.Update = false;
                _backInput.Update = false;
                _leftInput.Update = false;
                _rightInput.Update = false;
                _jumpInput.Update = false;
                _mouse0Input.Update = false;
                _mouse1Input.Update = false;
                _qInput.Update = false;
            }
            
        }
    }

    
    
    private void LateUpdate()
    {
        _cameraController.UpdatePosition(_cameraPositionRefTransform.position);
    }
}
