using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexAreaDamageOnContact : MonoBehaviour
{
    [System.Serializable]
    public class AreaElement
    {
        public WorldElements Element;
        public float Percentage;
    }

    public List<AreaElement> m_Elements = new List<AreaElement>();
    public float m_Damage = 50f;

    public bool m_IsPermanent = false;
    public float m_LifeTime;

    public void Start()
    {
        if (m_LifeTime <= 0f) m_LifeTime = 0.5f;
        if (!m_IsPermanent) Destroy(gameObject, m_LifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponentInParent<PlayerStats>();
            if (ps == null) return;

            ElementDamage[] elements = new ElementDamage[m_Elements.Count];
            for (int j = 0; j < m_Elements.Count; j++)
                elements[j] = new ElementDamage { Element = m_Elements[j].Element, Percentage = m_Elements[j].Percentage };

            ps.TakeDamage(
                m_Damage,
                elements,
                false,
                1.5f
            );
        }
    }
}
