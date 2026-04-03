using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bard_Aura : MonoBehaviour
{
    private Class_Bard m_Bard;
    void Awake()
    {
        m_Bard = GetComponentInParent<Class_Bard>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponentInChildren<PlayerStats>();

            foreach (WorldElements damage in System.Enum.GetValues(typeof(WorldElements)))
            {
                if (damage != WorldElements.Null) stats.AddDamageResistancePermanent(damage, m_Bard.m_BonusResistAllies);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponentInChildren<PlayerStats>();

            foreach (WorldElements damage in System.Enum.GetValues(typeof(WorldElements)))
            {
                if (damage != WorldElements.Null) stats.AddDamageResistancePermanent(damage, -m_Bard.m_BonusResistAllies);
            }
        }
    }
}
