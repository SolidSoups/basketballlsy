using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketBall : MonoBehaviour
{
    Rigidbody m_rb;
    public Rigidbody RB
    {
        get
        {
            return m_rb;
        }
    }
    
    bool m_isBeingLookedAt = false;
    Renderer m_renderer;
    
    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_renderer = GetComponentInChildren<Renderer>();
        m_renderer.material.SetFloat("_Outline", 0.0f); 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
