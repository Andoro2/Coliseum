using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerStats;

public class Race_Goliath : MonoBehaviour
{
    private PlayerStats m_PlayerStats;
    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel1 = false;
    public bool m_PassiveLevel10 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Pasiva nivel 10:")]
    public float m_LastDamageTime;
    public float m_ShieldCooldown = 5f;
    public float m_MaxShield;
    public float m_CurrentShield;

    [Header("Pasiva nivel 20:")]
    public GameObject m_DashDamageVisual;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;
        m_PlayerStats.OnDamageTaken += OnDamageTaken;

    }

    // Update is called once per frame
    void Update()
    {
        if (m_PassiveLevel10)
        {
            ReloadingShield();
        }
    }
    void ReloadingShield()
    {
        if (Time.time >= m_LastDamageTime + m_ShieldCooldown)
        {
            m_MaxShield = m_PlayerStats.m_MaxHealth * 0.1f;
            m_CurrentShield = m_MaxShield;
            m_PlayerStats.SetShield(PlayerStats.ShieldSource.GoliathShield, m_CurrentShield);
        }
    }
    void OnDamageTaken(float damage)
    {
        m_LastDamageTime = Time.time;
        m_CurrentShield = Mathf.Max(0f, m_CurrentShield - damage);
        m_PlayerStats.SetShield(ShieldSource.GoliathShield, m_CurrentShield);
    }
    void DashDamage()
    {
        Debug.Log("Goliath dash damage");
    }

    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 1 && !m_PassiveLevel1)
        {
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Cold, 0.5f);
            m_PassiveLevel1 = true;
        }
        if (newLevel >= 10 && !m_PassiveLevel10)
        {
            m_PassiveLevel10 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            transform.parent.GetComponent<PlayerController>().OnDashEnd += DashDamage;
            m_PlayerStats.AddDamageResistancePermanent(WorldElements.Cold, 0.5f);
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    private void OnDestroy()
    {
        m_PlayerStats.OnLevelUp -= OnLevelUp;
    }
}
