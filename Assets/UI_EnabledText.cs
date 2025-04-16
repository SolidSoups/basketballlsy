using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_EnabledText : MonoBehaviour
{
  [Header("References")]
  [SerializeField] TextMeshProUGUI m_textMesh;
  
  public bool IsEnabled
  {
    set
    {
      m_textMesh.enabled = value;
    }
    get
    {
      return m_textMesh.enabled;
    }
  }

  public string Text
  {
    set
    {
      m_textMesh.text = value;
    }
    get
    {
      return m_textMesh.text;
    }
  }

  void Awake()
  {
    IsEnabled = false;
  }

}
