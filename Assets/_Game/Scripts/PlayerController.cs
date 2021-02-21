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
    [SerializeField] private RopeController _ropeController;
    [SerializeField] private Transform _hookableTempTransform;

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

    private IHookable _currentHookable;

    private Vector3 _lastGroundedPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        Blackboard.Instance.OnPlayerAction(PlayerActionCool.WakesUp);
        _lastGroundedPosition = transform.position;
        _ropeController.SetActivateRope(false, null);
                
    }

    private void FixedUpdate()
    {

        CalculateFallDeath();
        MovementCalculations();
        ResetInputStartEndFlags();
    
        
        
        //////////////////////////////////

        void CalculateFallDeath()
        {

            if (_rigidbody.position.y < -20)
            {
                    OnPlayerIsDead();
                    Blackboard.Instance.OnPlayerAction(PlayerActionCool.Falls);
            }
        }
        
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
                        _lastGroundedPosition = transform.position;
                        
                        
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
            
            
            void CalculateSwing()
            {

                if (!_isSwinging && _isReadyToHook)
                {
                    if (_mouse0Input.Start)
                    {
                        SwingInputStartActions();
                        Blackboard.Instance.OnPlayerAction(PlayerActionCool.Swings);
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
                    _currentHookable?.OnHookStart(_hookableTempTransform);
                    Blackboard.Instance.CursorController.OnReadyToHookStateChange(HookCursorEnum.Hooked);
                    _ropeController.SetActivateRope(true, _hookableTempTransform);
                    _maxTetherLength = (_rigidbody.position - _hookableTempTransform.position).magnitude;
                    _isOnAirAfterSwinging = false;
                    _isSwinging = true;
                }

                void SwingInputUpdateActions()
                {
                     _currentHookable?.OnHookUpdate(_hookableTempTransform);
                     _isOnGroundedAfterSwinging = false;
                     
                  //  Debug.DrawLine(_rigidbody.position, _hookableMeta.TransformToFollow.position, Color.green);
                  //  Debug.DrawRay(_rigidbody.position, _rigidbody.velocity, Color.red);
                    Vector3 forwardVec = _cameraController.ForwardVector;
                    Vector3 rightVec = _cameraController.RightVector;

                    rightVec *= _maxSwingInputForce * _inputDirection.x;
                    forwardVec *= _maxSwingInputForce * _inputDirection.y;
                    Vector3 sumVec = rightVec + forwardVec;
                    sumVec = Vector3.ClampMagnitude(sumVec, _maxSwingInputForce);
                    
                    _rigidbody.AddForce(sumVec, ForceMode.Acceleration);

                    if (_isGrounded)
                    {
                        _rigidbody.AddForce( (_hookableTempTransform.position - _rigidbody.position).normalized * _tetherTentionForce, ForceMode.Acceleration );
                    }
                    
                    Vector3 newPos = _rigidbody.position + _rigidbody.velocity * Time.fixedDeltaTime;
                    Vector3 tetherToNewPos = (newPos - _hookableTempTransform.position);
                    tetherToNewPos = Vector3.ClampMagnitude(tetherToNewPos, _maxTetherLength);

                    if (tetherToNewPos.magnitude < _maxTetherLength)
                    {
                        _maxTetherLength = tetherToNewPos.magnitude;
                    }
                    
                    
                    _rigidbody.velocity = (tetherToNewPos - _rigidbody.position + _hookableTempTransform.position).normalized *  _rigidbody.velocity.magnitude;

                    _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _swingingMaxSpeed);
                    _rigidbody.velocity -= (Time.fixedDeltaTime * _swingingFriction * _rigidbody.velocity);
                        
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

    void SwingInputEndActions()
    {
        _ropeController.SetActivateRope(false, null);
        _currentHookable?.OnHookEnd(_hookableTempTransform);
        _isSwinging = false;
        _isOnAirAfterSwinging = true;
        Blackboard.Instance.CursorController.OnReadyToHookStateChange(HookCursorEnum.NotHookable);

        CalculateIsReadyToHook();
                    
                  
    }
    
    void CalculateIsReadyToHook()
    {
        bool isReadyToHookLocal = _isReadyToHook;
        _isReadyToHook = false;
        if (!_isSwinging)
        {
            _currentHookable = null;
            Ray ray = new Ray(_cameraController.transform.position, _cameraController.ForwardVector);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _hookRaycastLayerMask))
            {
                IHookable hookable = hitInfo.collider.GetComponentInParent<IHookable>();

                if (hookable != null)
                {
                    bool canHook = hookable.TryToGetHookableCondition(hitInfo, ray);
                    if (canHook)
                    {
                        _currentHookable = hookable;
                        _isReadyToHook = canHook;
                    }

                            
                }
                else
                {
                    Debug.LogError("ERROR this shouldnt happen -> " + hitInfo.collider.gameObject.name);
                }
            }
                    
                    
            if (_isReadyToHook != isReadyToHookLocal)
            {
                Blackboard.Instance.CursorController.OnReadyToHookStateChange(_isReadyToHook ? HookCursorEnum.ReadyToHook : HookCursorEnum.NotHookable);
            }
                    
        }
                
                
                
    }

    // Update is called once per frame
    void Update()
    {
        SetInputFlags();
        CalculateTimeSlowDown();

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _rigidbody.position = _lastGroundedPosition;
        }
        
        void CalculateTimeSlowDown()
        {
            if (_qInput.Start)
            {
                Blackboard.Instance.SlowTime();
                Blackboard.Instance.OnPlayerAction(PlayerActionCool.SlowsTime);
            }

            if (_qInput.End)
            {
                Blackboard.Instance.SpeedTime();
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


    void OnPlayerIsDead()
    {
        _rigidbody.position = _lastGroundedPosition;
        _rigidbody.velocity = Vector3.zero;

        if (_isSwinging)
        {
            SwingInputEndActions();
        }
        Blackboard.Instance.OnPlayerKilled();
    }
    
    private void LateUpdate()
    {
        _cameraController.UpdatePosition(_cameraPositionRefTransform.position);
    }

    public void OnAttachedHookableObjectDestroyed()
    {
       SwingInputEndActions();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            OnPlayerIsDead();
            Blackboard.Instance.OnPlayerAction(PlayerActionCool.GetHitsByABullet);
        }
    }
}
