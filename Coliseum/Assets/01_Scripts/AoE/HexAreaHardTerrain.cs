using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HexAreaHardTerrain : MonoBehaviour
{
    public float m_SlowPercent = 0.5f;

    public bool m_Permanent = false;
    public float m_LifeTime;

    private HashSet<GameObject> m_AffectedBeings = new HashSet<GameObject>();

    public void Start()
    {
        if (m_LifeTime <= 0f) m_LifeTime = 0.5f;
        if (!m_Permanent) Destroy(gameObject, m_LifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponentInChildren<PlayerStats>();

            if (pc == null || pc.IsImmuneTo(StatusEffect.Slow)) return;

            pc.ApplySpeedBonus(-m_SlowPercent, "Slowed");
            m_AffectedBeings.Add(other.transform.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            var stats = other.GetComponentInChildren<EnemyStats>();
            if (stats == null || stats.IsImmuneTo(StatusEffect.Slow)) return;

            stats.ApplySpeedBonus(-m_SlowPercent);
            m_AffectedBeings.Add(other.transform.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_AffectedBeings.Contains(other.transform.gameObject))
        {
            if (other.CompareTag("Player"))
            {
                var ps = other.GetComponentInChildren<PlayerStats>();
                if (ps.IsImmuneTo(StatusEffect.Slow)) return;
                else ps.ApplySpeedBonus(m_SlowPercent, null);
            }
            else if (other.CompareTag("Enemy"))
            {
                var es = other.GetComponentInParent<EnemyStats>();
                if (es.IsImmuneTo(StatusEffect.Slow)) return;
                else es.ApplySpeedBonus(m_SlowPercent);
            }

            m_AffectedBeings.Remove(other.transform.gameObject);
        }
        else return;
    }

    private void OnDestroy()
    {
        foreach (var being in m_AffectedBeings)
        {
            if (being.transform.gameObject.CompareTag("Player")) being.GetComponentInChildren<PlayerStats>().ApplySpeedBonus(m_SlowPercent, null);
            else if (being.transform.gameObject.CompareTag("Enemy")) being.GetComponentInChildren<EnemyStats>().ApplySpeedBonus(m_SlowPercent);
        }
        m_AffectedBeings.Clear();
    }
}
