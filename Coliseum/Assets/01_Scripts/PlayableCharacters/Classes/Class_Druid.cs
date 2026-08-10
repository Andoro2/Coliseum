using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyStats;

public class Class_Druid : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Level 8")]
    public float m_ArmorShredPercent = 0.1f;
    public float m_ArmorShredDuration = 10f;
    [Header("Level 12")]
    public float m_ElementalDmgHealPercentage = 0.05f;
    [Header("Level 16")]
    public float m_CritIncreasePercentage = 0.1f;
    public float m_RootDuration = 2f;
    public GameObject m_CritStunVFX;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Druid);

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;

        EnemyStats.OnAnyEnemyDamaged += HandleAnyEnemyDamaged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 4 && !m_PassiveLevel4)
        {
            m_PlayerStats.SetImmunity(StatusEffect.Slow, true);

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            m_PassiveLevel8 = true;
        }
        if (newLevel >= 12 && !m_PassiveLevel12)
        {
            m_PassiveLevel12 = true;
        }
        if (newLevel >= 16 && !m_PassiveLevel16)
        {
            m_PlayerStats.GetCritChance(m_CritIncreasePercentage);

            m_PassiveLevel16 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    // lvl 8
    private void HandleAnyEnemyDamaged(EnemyStats target, float damage, WorldElements element, bool isCrit, EnemyStats.Killer source)
    {
        if (m_PassiveLevel8)
        {
            target.ApplyArmorReduction(
                EnemyArmorReductionSource.DruidLevel8,
                m_ArmorShredPercent,
                m_ArmorShredDuration
            );
        }
        if (m_PassiveLevel12 && element != WorldElements.Null)
        {
            m_PlayerStats.Heal(damage * m_ElementalDmgHealPercentage);
        }
        if (m_PassiveLevel16 && isCrit)
        {
            target.ApplyStun(m_RootDuration);

            if (m_CritStunVFX != null)
                Instantiate(m_CritStunVFX, target.transform.position, Quaternion.identity);
        }
    }

    private void OnDestroy()
    {
        m_PlayerStats.OnLevelUp -= OnLevelUp;
        EnemyStats.OnAnyEnemyDamaged -= HandleAnyEnemyDamaged;
    }
}
