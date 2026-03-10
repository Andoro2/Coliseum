using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Class_Barbarian : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Pasiva nivel 10:")]
    public float m_SpeedBonusPercent;
    public float m_AttackSpeedBonusPercent;

    [Header("Pasiva nivel 12:")]
    public float m_TrapDodgePercent;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_PassiveLevel4)
            ApplyWrathBonus();
    }

    private void StackWrath(float damage)
    {
        m_PlayerStats.StackWrath(damage * 0.1f);
    }

    void ApplyWrathBonus()
    {
        m_PlayerStats.SetDynamicDamageBonus(PlayerStats.DynamicDamageSource.BarbarianWrath, m_PlayerStats.GetWrathStatBonus());
        m_PlayerStats.SetDynamicLifeRegenBonus(PlayerStats.DynamicLifeRegenSource.BarbarianWrath, m_PlayerStats.GetWrathStatBonus());
    }

    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 4 && !m_PassiveLevel4)
        {
            m_PlayerStats.OnDamageTaken += StackWrath;

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            m_PlayerStats.ApplySpeedBonus(m_SpeedBonusPercent);
            m_PlayerStats.ApplyAttackSpeedBonus(m_AttackSpeedBonusPercent);
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Critical, 0.25f);
            m_PassiveLevel8 = true;
        }
        if (newLevel >= 12 && !m_PassiveLevel12)
        {
            m_PassiveLevel12 = true;
        }
        if (newLevel >= 16 && !m_PassiveLevel16)
        {
            m_PassiveLevel16 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PlayerStats.m_MaxWrath += 50f;
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
}
