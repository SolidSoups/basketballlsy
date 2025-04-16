using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BallManipulator : MonoBehaviour
{
  [Header("Setttings")]
  [SerializeField] float m_shortThrowForce = 10.0f; // Newtons
  [SerializeField] float m_shortFocusLength = 5.0f; // Meters
  
  [SerializeField] float m_longThrowForce = 20.0f; // Newtons
  [SerializeField] float m_longFocusLength = 10.0f; // Meters
  
  [SerializeField] float m_throwDelay = 2.0f; // seconds
  [SerializeField] float m_ballMass = 0.7f;
  [SerializeField, Range(10, 100)] int LinePoints = 25;
  [SerializeField, Range(0.01f, 0.25f)] float TimeBetweenPoints = 0.1f;

  [SerializeField] LayerMask m_projectionLayerMask;
  
  
  [Header("References")]
  [SerializeField] Transform m_orientation;
  [SerializeField] Transform m_ballBone; // Where the ball will be held
  [SerializeField] UI_EnabledText ui_throwEnabledText;
  [SerializeField] UI_EnabledText ui_throwCooldownEnabledText;
  
  
  // tmp
  [Header("Prefabs")]
  [SerializeField] GameObject m_basketBallPrefab;
  
  
  // private
  bool b_isThrowCooldownOn = false;
  bool b_isThrowing = false;

  private enum ThrowType
  {
    SHORT,
    LONG
  };

  ThrowType m_throwType = ThrowType.SHORT;
  Vector3 latestHitPos = Vector3.zero;

  // Start is called before the first frame update
  void Start()
  { }

  // Update is called once per frame
  void Update()
  {
    if (b_isThrowCooldownOn || b_isThrowing)
      return; 
    
    if (Input.GetMouseButton(0)){
      m_throwType = ThrowType.SHORT;
      StartCoroutine(StartThrow("SHORT", 0, m_shortThrowForce));
    }
    else if (Input.GetMouseButton(1)){
      m_throwType = ThrowType.LONG;
      StartCoroutine(StartThrow("LONG", 1, m_longThrowForce));
    }
  }

  IEnumerator StartThrow(string throwText, int upMouseCode, float force)
  {
    // State changes
    b_isThrowing = true;
    
    // Init a ball
    var newBall = Instantiate(m_basketBallPrefab, m_ballBone.position, Quaternion.identity, m_ballBone);
    var rb = newBall.GetComponent<Rigidbody>();
    rb.isKinematic = true;
    
    // Set UI text
    ui_throwEnabledText.IsEnabled = true;
    ui_throwEnabledText.Text = throwText;
    
    // Wait for throw
    while (!Input.GetMouseButtonUp(upMouseCode)){
      Vector3 initialBallVel = Vector3.zero;
      switch (m_throwType){
        case ThrowType.SHORT:
          initialBallVel = GetThrowDir(m_shortFocusLength) * Mathf.Pow(m_shortThrowForce, 2) * m_ballMass;  
          break;
        case ThrowType.LONG:
          initialBallVel = GetThrowDir(m_longFocusLength) * Mathf.Pow(m_longThrowForce, 2) * m_ballMass;  
          break;
      }

      (Vector3 hitPos, bool failed) = FindEndPos(m_ballBone.position, initialBallVel);
      if (!failed){
        Debug.Log("DID NOT FAIL");
        latestHitPos = hitPos;
      }
      
      yield return null;
    }
    
    // Reset ball
    rb.isKinematic = false;
    newBall.transform.parent = null;
    
    // Reset UI
    ui_throwEnabledText.IsEnabled = false;
    
    // State reset
    b_isThrowing = false;
      
    // Self-explanitory 
    ThrowBall(newBall, force);
    StartCoroutine(StartDelay());
  }

  IEnumerator StartDelay()
  {
    b_isThrowCooldownOn = true;
    ui_throwCooldownEnabledText.IsEnabled = true;
    float accumulator = 0;
    while (accumulator < m_throwDelay){
      accumulator += Time.deltaTime;
      ui_throwCooldownEnabledText.Text = $"{m_throwDelay - accumulator}s"; 
      yield return null;
    }
    ui_throwCooldownEnabledText.IsEnabled = false;
    b_isThrowCooldownOn = false;
  }

  void ThrowBall(GameObject ball, float force)
  {
    Vector3 vForce = GetThrowDir(m_shortFocusLength) * force * 100.0f;
    ball.GetComponent<Rigidbody>().AddForce(vForce); 
    Destroy(ball, 5.0f);
  }

  Vector3 GetThrowDir(float focusLength)
  {
    Vector3 vForward = m_orientation.position + m_orientation.forward * focusLength;
    Vector3 focusedForward = vForward - m_ballBone.position;
    return focusedForward.normalized;
  }
  
  void OnDrawGizmos()
  {
    if (!m_ballBone)
      return;
    
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(m_ballBone.position, 0.05f);

    if (!b_isThrowing && !b_isThrowCooldownOn)
      return;

    Gizmos.DrawWireSphere(latestHitPos + new Vector3(0, 0.35f, 0), 0.35f);

  }
  
  ( Vector3 pos, bool failed  )FindEndPos(Vector3 startPosition, Vector3 startVelocity)
  {
    Func<float, Vector3> getPos = (time) =>
    {
      Vector3 point = startPosition + time * startVelocity;
      point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);
      return point;
    }; 
    
    float time = 0;
    while(true){
      Vector3 currentPoint = getPos(time) + new Vector3(0, -0.35f, 0);
      time += TimeBetweenPoints;
      Vector3 nextPoint = getPos(time) + new Vector3(0, -0.35f, 0);
      
      // lets do some raycasting
      Vector3 vecDiff = nextPoint - currentPoint;
      if (Physics.Raycast(currentPoint, vecDiff, out RaycastHit hit, vecDiff.magnitude, m_projectionLayerMask)){
        return (hit.point, false);
      }
      
      if (nextPoint.y <= 0){
        break;
      }
    }
    return (Vector3.zero, true);
  }

  // Generates a list of positions for a parabola relative to origin. Offset each point by the initial position.
  List<Vector3> GenerateParabola(Vector3 p0, Vector3 v0)
  {
    List<Vector3> points = new();
    points.Add(p0);

    float time = 0;
    while(true){
      time += TimeBetweenPoints;
      Vector3 point = p0 + time * v0;
      point.y = p0.y + v0.y * time + (Physics.gravity.y / 2f * time * time);
      
      if (point.y <= 0){
        break;
      }

      points.Add(point);
    }

    return points;
  }
}
