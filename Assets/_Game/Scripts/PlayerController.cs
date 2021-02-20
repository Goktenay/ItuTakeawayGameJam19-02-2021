using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Transform _groundedRayCastTransform;
    
    [Header("Settings")] 
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _jumpSpeed;
    [SerializeField] private float _upwardsGravity;
    [SerializeField] private float _downwardsGravity;
    
    
    private Vector3 _velocity = Vector3.zero;
    private Vector2Int _inputDirection = Vector2Int.zero;
    private bool _isGrounded;
    private float _currentGravity;
    
    private InputFlag _jumpInput = new InputFlag(KeyCode.Space);
    private InputFlag _leftInput = new InputFlag(KeyCode.LeftArrow, KeyCode.A);
    private InputFlag _rightInput = new InputFlag(KeyCode.RightArrow, KeyCode.D);
    private InputFlag _backInput = new InputFlag(KeyCode.DownArrow, KeyCode.S);
    private InputFlag _forwardInput = new InputFlag(KeyCode.UpArrow, KeyCode.W);
    
    
    
    
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
            CalculateVelocity();
            CalculateOnGrounded();
            CalculateJump();
            CalculateGravity();
            

            
            
            
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
            
            void CalculateVelocity()
            {
                
                float yawValue =_cameraController.CameraYawValue * Mathf.Deg2Rad;
                Vector2 forwardVec = new Vector2(Mathf.Sin(yawValue), Mathf.Cos(yawValue));
                forwardVec.Normalize();
                Vector2 rightVec = new Vector2(forwardVec.y, -forwardVec.x);
                
                Vector2 horizontalVel = _inputDirection.x * _maxSpeed * rightVec;
                Vector2 verticalVel = _inputDirection.y * _maxSpeed * forwardVec;
                Vector2 xzSum = Vector2.ClampMagnitude(horizontalVel + verticalVel, _maxSpeed);

                _velocity = _rigidbody.velocity;
                _velocity.x = xzSum.x;
                _velocity.z = xzSum.y;
                _rigidbody.velocity = _velocity;

                
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
                    _velocity = _rigidbody.velocity;
                    _velocity.y = _jumpSpeed;
                    _isGrounded = false;
                    _rigidbody.velocity = _velocity;
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
                    }
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
        }
    }


    // Update is called once per frame
    void Update()
    {
        SetInputFlags();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

            }
            void ResetInputUpdateFlags()
            {
                _forwardInput.Update = false;
                _backInput.Update = false;
                _leftInput.Update = false;
                _rightInput.Update = false;
                _jumpInput.Update = false;
            }
            
        }
    }

    
    
    private void LateUpdate()
    {
        _cameraController.UpdatePosition(_cameraPositionRefTransform.position);
    }
}
