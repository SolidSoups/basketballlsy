using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrengthSlider : MonoBehaviour
{
  [Header("References")]
  [SerializeField] Image m_sliderImage;
  [SerializeField] Image m_backImage;

  bool m_isEnabled = false;
  public bool IsEnabled
  {
    set
    {
      m_sliderImage.enabled = value;
      m_backImage.enabled = value;
      m_isEnabled = value;
    }
    get
    {
      return m_isEnabled;
    }
  }

  void Awake()
  {
    IsEnabled = false;
  }

  public float Fill
  {
    set
    {
      if (!m_isEnabled)
        IsEnabled = true;
      m_sliderImage.fillAmount = Mathf.Clamp01(value);
    }
  }
}
