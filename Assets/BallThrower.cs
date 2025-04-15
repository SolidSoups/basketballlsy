using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BallThrower : MonoBehaviour
{
  [Header("Prefabs")]
  [SerializeField] BasketBall m_basketBall;

  [Header("Settings")]
  [SerializeField] Transform m_ballSpawnTransform;
  
  [SerializeField] float m_minForce = 2f;
  [SerializeField] float m_maxForce = 5f;
  [SerializeField] float m_throwTime = 2f; // Seconds
  [SerializeField] Vector3 m_offsetAnimationPosition;

  [Header("References")]
  [SerializeField] Transform m_orientation;

  [SerializeField] StrengthSlider m_strengthSlider;


  // Start is called before the first frame update
  void Start()
  { }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetMouseButtonDown(0)){
      StartCoroutine(StartStrenghtTest());
    }
  }

  void OnDrawGizmos()
  {
    if (m_ballSpawnTransform){
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(m_ballSpawnTransform.position, 0.05f);
    }
  }

  IEnumerator StartStrenghtTest()
  {
    // quicker access to easing function
    Func<float, float> easingFunc = easeInOutExpo;

    // Create ball
    var newBall = Instantiate(m_basketBall, m_ballSpawnTransform.position, Quaternion.identity);
    // Set ball settings
    newBall.RB.isKinematic = true;
    newBall.transform.parent = m_ballSpawnTransform;
    m_strengthSlider.IsEnabled = true;
    // Collect ball pos
    Vector3 oldBallPos = newBall.transform.localPosition; 
    
    float acc = 0;
    while (acc < m_throwTime){
      // calc accumulation and easing
      acc += Time.deltaTime;
      float t = acc / m_throwTime;
      float betterT = easingFunc(t);
      
      // set UI slider fill
      m_strengthSlider.Fill = betterT;
      Vector3 newBallPos = Vector3.Lerp(oldBallPos, oldBallPos + m_offsetAnimationPosition, betterT);
      newBall.transform.localPosition = newBallPos;
      

      // check for break case
      if (Input.GetMouseButtonUp(0)){
        break;
      }
      yield return null;
    }

    // reset some settings
    m_strengthSlider.IsEnabled = false;
    newBall.transform.parent = null;
    newBall.RB.isKinematic = false;
    
    // calculate yield and throw ball
    float strengthYield = Mathf.Lerp(m_minForce, m_maxForce, easingFunc(acc / m_throwTime));
    ThrowBall(newBall, strengthYield);
  }

  float easeInSine(float x) => 1 - Mathf.Cos((x * Mathf.PI) / 2);

  float easeInOutExpo(float x)
  {
    if (x == 0f)
      return 0f;
    if (x == 1f)
      return 1f;

    if (x < 0.5f)
      return Mathf.Pow(2, 20 * x - 10)/ 2.0f;

    return (2 - Mathf.Pow(2, -20 * x + 10)) / 2.0f;
  }

  void ThrowBall(BasketBall ball, float strength)
  {
    ball.RB.AddForce(m_orientation.forward.normalized * (strength * 100f));
    Destroy(ball.gameObject, 5f /*seconds*/);
  }
}
