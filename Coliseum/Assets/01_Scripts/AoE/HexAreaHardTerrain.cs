using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexAreaHardTerrain : MonoBehaviour
{
    public float m_SlowPercent = 0.5f;

    public bool m_Permanent = false;
    public float m_LifeTime;

    public void Start()
    {
        if (m_LifeTime <= 0f) m_LifeTime = 0.5f;
        if (!m_Permanent) Destroy(gameObject, m_LifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!IsServer) return; // solo el servidor gestiona la colisión

        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc == null) return;

            bool isDruidL4 = other.GetComponentInChildren<Class_Druid>().m_PassiveLevel4;

            if (isDruidL4) return;
            else other.GetComponentInChildren<PlayerStats>().ApplySpeedBonus(-m_SlowPercent);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (!IsServer) return; // solo el servidor gestiona la colisión

        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc == null) return;

            bool isDruidL4 = other.GetComponentInChildren<Class_Druid>().m_PassiveLevel4;

            if (isDruidL4) return;
            else other.GetComponentInChildren<PlayerStats>().ApplySpeedBonus(m_SlowPercent);
        }
    }
}
