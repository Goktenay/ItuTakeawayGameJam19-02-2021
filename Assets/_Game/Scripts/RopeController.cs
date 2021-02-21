using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RopeController : MonoBehaviour
{
   [Header("Dependencies")]
   [SerializeField] private LineRenderer _lineRenderer;
   [Space(10)]
   [SerializeField] private Transform _startAnchorTransform;
   [SerializeField] private Transform _endAnchorTransform;
   
   [Header("Simulation Settings")] 
   [SerializeField] private Vector3 _gravity = new Vector3(0f, -1.5f, 0);
   [SerializeField] private float _segmentLength = 0.25f;
   [SerializeField] private int _segmentCount = 35;
   [SerializeField] private int _constraintIterationCount = 10;
   
   [Header("Rendering Settings")]
   [SerializeField] private float _lineWidth = 0.1f;

   [Header("Debug Settings")] 
   [SerializeField] private bool _showGizmos;
   [SerializeField] private bool _showStartAndEndPoints;
   [SerializeField] private bool _showRopeSegments;
   [SerializeField] private bool _showRopeEdges;
   [SerializeField] private float _gizmoSphereRadius = 0.1f;
   

   private List<RopeSegment> _ropeSegments = new List<RopeSegment>();
   private RopeType _ropeType = RopeType.Free;

   private Vector3 Gravity => _gravity;

   private bool _isRopeActive;
   private RopeSegment _lastRopeSegment;
   private bool _isTweenEnded;

   private GameObject _endTransformCopyGameObject;
   
   private void Start()
   {
      InitializeInitialRopeSettings();
   }

   private void Update()
   {
      if (_isRopeActive)
      {
         SimulateRope();
         DrawRope();
      }
   }

   public void SetActivateRope(bool val, Transform endTransform)
   {
      DOTween.Kill("RopeJob");
      
      if (val)
      {
         if (_endTransformCopyGameObject != null)
         {
            Destroy(_endTransformCopyGameObject);
         }
         
         _endTransformCopyGameObject = Instantiate(endTransform.gameObject, endTransform.parent);
         _endTransformCopyGameObject.transform.position = endTransform.position;
         
         _lineRenderer.enabled = true;
         _isRopeActive = true;

         foreach (var VARIABLE in _ropeSegments)
         {
            VARIABLE.CurrentPosition = _startAnchorTransform.position;
            VARIABLE.OldPosition = _startAnchorTransform.position;
         }
         
       
         Vector3 startPos = _startAnchorTransform.position;

         GameObject newGameObject = new GameObject();
         newGameObject.transform.position = _startAnchorTransform.position;
         _endAnchorTransform = newGameObject.transform;
         
         DOVirtual.Float(0, 1, 0.25f, onVirtualUpdate: value =>
            {
               newGameObject.transform.position =
                  Vector3.Lerp(startPos, _endTransformCopyGameObject.transform.position, value);
            }).OnKill(() =>
            {
               _endAnchorTransform = _endTransformCopyGameObject.transform;
            })
            .SetId("RopeJob");

      }
      else
      {
         _lineRenderer.enabled = false;
         _isRopeActive = false;
      }

   }

   private void InitializeInitialRopeSettings()
   {
      CalculateRopeType();
      InitializeRopeSegments();
   }
   private void CalculateRopeType()
   {
      if (_startAnchorTransform == null)
      {
         Debug.Assert(_endAnchorTransform != null, "Rope only has end pivot transform!");
         _ropeType = RopeType.Free;
      }
      else
      {
         if (_endAnchorTransform == null)
         {
            _ropeType = RopeType.OneSideJointed;
         }
         else
         {
            _ropeType = RopeType.TwoSideJointed;
         }
      }
   }
   private void InitializeRopeSegments()
   {
      Vector3 ropeStartPoint = Vector3.zero;
      Vector3 ropeEndPoint = Vector3.zero;
      
      CreateAndInitializeRopeStartAndEndPoints();
      CreateAndInitializeRopeSegments();
      
      void CreateAndInitializeRopeStartAndEndPoints()
      {
         switch (_ropeType)
         {
            case RopeType.Free:
               ropeStartPoint = transform.position;
               ropeEndPoint = ropeStartPoint + _segmentCount * _segmentLength * Vector3.down;
               break;
         
            case RopeType.OneSideJointed:
               ropeStartPoint = _startAnchorTransform.position; 
               ropeEndPoint = ropeStartPoint + _segmentCount * _segmentLength * Vector3.down;
               break;
         
            case RopeType.TwoSideJointed:
               ropeStartPoint = _startAnchorTransform.position;
               ropeEndPoint = _endAnchorTransform.position;
               break;
         }
      }
      void CreateAndInitializeRopeSegments()
      {
         for (int i = 0; i < _segmentCount; i++)
         {
            float lerpValue = (float) i / (_segmentCount - 1);
            Vector3 ropePos = Vector3.Lerp(ropeStartPoint, ropeEndPoint, lerpValue);
            RopeSegment segment = new RopeSegment(ropePos);
            _ropeSegments.Add(segment);
         }

         _lastRopeSegment = _ropeSegments[_segmentCount - 1];
         
         if (_segmentCount > 0)
         {
            switch (_ropeType)
            {
               case RopeType.Free: 
                  break;
               
               case RopeType.OneSideJointed:
                  _ropeSegments[0].IsPositionFixed = true;
                  break;
               
               case RopeType.TwoSideJointed:
                  _ropeSegments[0].IsPositionFixed = true;
                  _ropeSegments[_segmentCount - 1].IsPositionFixed = true;
                  break;
            }  
         }
      }
   }
   
   private void DrawRope()
   {
      float lineWidth = _lineWidth;
      _lineRenderer.startWidth = lineWidth;
      _lineRenderer.endWidth = lineWidth;

      Vector3[] ropePositions = new Vector3[_segmentCount];
      for (int i = 0; i < _segmentCount; i++)
      {
         ropePositions[i] = _ropeSegments[i].CurrentPosition;
      }

      _lineRenderer.positionCount = ropePositions.Length;
      _lineRenderer.SetPositions(ropePositions);
   }
   
   private void SimulateRope()
   {
      CalculateTheFirstAndLastSegmentPositions();
      Simulate();

      
      for (int i = 0; i < _constraintIterationCount; i++)
      {
         ApplyConstraints();
      }

      void Simulate()
      {
         Vector3 gravity = Gravity;

         for (int i = 0; i < _segmentCount; i++)
         {
            RopeSegment segment = _ropeSegments[i];
            if(segment.IsPositionFixed) continue;
         
            Vector3 velocity = segment.CurrentPosition - segment.OldPosition;
            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += velocity * Time.timeScale;
            segment.CurrentPosition += gravity * Time.fixedDeltaTime;
         }
      }
      void CalculateTheFirstAndLastSegmentPositions()
      {
         switch (_ropeType)
         {
            case RopeType.Free: 
               break;
         
            case RopeType.OneSideJointed:
               _ropeSegments[0].CurrentPosition = _startAnchorTransform.position;
               _ropeSegments[0].OldPosition = _startAnchorTransform.position;
               break;
         
            case RopeType.TwoSideJointed:
               _ropeSegments[0].CurrentPosition = _startAnchorTransform.position;
               _ropeSegments[0].OldPosition = _startAnchorTransform.position;
               _ropeSegments[_segmentCount - 1].CurrentPosition = _endAnchorTransform.position;
               _ropeSegments[_segmentCount - 1].OldPosition = _endAnchorTransform.position;
               break;
         }
      }
      void ApplyConstraints()
      {
         Action<RopeSegment, RopeSegment, Vector3, int> ropeConstraintAction;
         ropeConstraintAction = GetRopeConstraintAction();
         ApplyRopeConstraint(ropeConstraintAction);
         
         // TODO: Maybe carry out apply rope constraint function to outside or decide it when the rope type is set
         void ApplyRopeConstraint(Action<RopeSegment, RopeSegment, Vector3, int> constraintAction)
         {
            for (int i = 0; i < _segmentCount - 1; i++)
            {
               RopeSegment firstSeg = _ropeSegments[i];
               RopeSegment secondSeg = _ropeSegments[i + 1];

               Vector3 currentSecondToFirstVec = firstSeg.CurrentPosition - secondSeg.CurrentPosition;
               float dist = currentSecondToFirstVec.magnitude;
            
               float error = Mathf.Abs(dist - _segmentLength);
               Vector3 changeDir = currentSecondToFirstVec.normalized * (dist >= _segmentLength ? 1 : -1);

               Vector3 changeAmount = changeDir * error;

               constraintAction(firstSeg, secondSeg, changeAmount, i);
            }
         }
         Action<RopeSegment, RopeSegment, Vector3, int> GetRopeConstraintAction()
         {
            switch (_ropeType)
            {
               case RopeType.Free: return FreeRopeConstraint;
                  break;
               case RopeType.OneSideJointed: return OneSideJointedRopeConstraint;
                  break;
               case RopeType.TwoSideJointed: return TwoSideJointedRopeConstraint;
                  break;
               default: return FreeRopeConstraint;
                  break;
            }
         }
         
         void FreeRopeConstraint(RopeSegment first, RopeSegment second, Vector3 changeAmount, int i)
         {
            first.CurrentPosition -= changeAmount * 0.5f;
            second.CurrentPosition += changeAmount * 0.5f;
         }
         
         void OneSideJointedRopeConstraint(RopeSegment first, RopeSegment second, Vector3 changeAmount, int i)
         {
            if (i != 0)
            {
               first.CurrentPosition -= changeAmount * 0.5f;
               second.CurrentPosition += changeAmount * 0.5f;  
            }
            else
            {
               second.CurrentPosition += changeAmount;  
            }
         }
         
         void TwoSideJointedRopeConstraint(RopeSegment first, RopeSegment second, Vector3 changeAmount, int i)
         {
            if (i == _segmentCount - 2)
            {
               first.CurrentPosition -= changeAmount;
            }
            else if (i == 0)
            {
               second.CurrentPosition += changeAmount;
            } 
            else
            {
               first.CurrentPosition -= changeAmount * 0.5f;
               second.CurrentPosition += changeAmount * 0.5f;
            }
         }
      }
   }

   private void OnDrawGizmos()
   {
      if (_showGizmos && _ropeSegments != null && _ropeSegments.Count != 0)
      {
         if (_showRopeSegments)
         {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _ropeSegments.Count ; i++)
            {
               Gizmos.DrawSphere(_ropeSegments[i].CurrentPosition, _gizmoSphereRadius);
            }
         }
         
         if (_showRopeEdges)
         {
            Gizmos.color = Color.white;
            for (int i = 0; i < _ropeSegments.Count - 1; i++)
            {
               Vector3 formerPos = _ropeSegments[i].CurrentPosition;
               Vector3 latterPos = _ropeSegments[i + 1].CurrentPosition;
               Gizmos.DrawLine(formerPos, latterPos);
            }
         }
         
         if (_showStartAndEndPoints)
         {
            if (_startAnchorTransform != null)
            {
               Gizmos.color = Color.green;
               Gizmos.DrawSphere(new Vector3(_startAnchorTransform.position.x, _startAnchorTransform.position.y,0), _gizmoSphereRadius);
            }
            
            if (_endAnchorTransform != null)
            {
               Gizmos.color = Color.red;
               Gizmos.DrawSphere(new Vector3(_endAnchorTransform.position.x, _endAnchorTransform.position.y,0), _gizmoSphereRadius);
            }
         }
      }
   }


   // TODO: Maybe make struct instead of class?
   class RopeSegment
   {
      public Vector3 OldPosition;
      public Vector3 CurrentPosition;
      public bool IsPositionFixed;
      
      public RopeSegment(Vector3 position)
      {
         OldPosition = position;
         CurrentPosition = position;
      }
   }
   
   public enum RopeType {OneSideJointed, TwoSideJointed, Free}
 
}
