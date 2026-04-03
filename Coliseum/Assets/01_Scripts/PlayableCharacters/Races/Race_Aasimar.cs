using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Race_Aasimar : MonoBehaviour
{
    private PlayerStats m_PlayerStats;
    public float m_LifeRegenBonusPercent = 0f;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel1 = false,
        m_PassiveLevel10 = false, HandsApplied = false,
        m_PassiveLevel20 = false;

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
        if (m_PassiveLevel10)
        {
            Hands();
        }
    }
    void Hands()
    {
        if(m_PlayerStats.m_CurrentHealth <= m_PlayerStats.m_MaxHealth * 0.3)
        {
            if (!HandsApplied)
            {
                m_PlayerStats.m_LifeRegenBonusPercent += m_LifeRegenBonusPercent;
                HandsApplied = true;
            }
        }
        else if (HandsApplied)
        {
            m_PlayerStats.m_LifeRegenBonusPercent -= m_LifeRegenBonusPercent;
            HandsApplied = false;
        }
    }

    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 1 && !m_PassiveLevel1)
        {
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Necrotic, 0.5f);
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Radiant, 0.5f);
            m_PassiveLevel1 = true;
        }
        if (newLevel >= 10 && !m_PassiveLevel10)
        {
            m_PassiveLevel10 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Necrotic, 0.5f);
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Radiant, 0.5f);
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    private void OnDestroy()
    {
        m_PlayerStats.OnLevelUp -= OnLevelUp;
    }
}
