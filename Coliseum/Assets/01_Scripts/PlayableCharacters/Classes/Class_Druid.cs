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

    [Header("Lvl 8")]
    public float m_ArmorShredPercent = 0.1f;
    public float m_ArmorShredDuration = 10f;
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
            m_PassiveLevel16 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    // lvl 8
    private void HandleAnyEnemyDamaged(EnemyStats target, float damage, WorldElements element, EnemyStats.Killer source)
    {
        if (!m_PassiveLevel8) return;
        if (source != EnemyStats.Killer.Player) return;

        target.ApplyArmorReduction(
            ArmorReductionSource.DruidLevel8,
            m_ArmorShredPercent,
            m_ArmorShredDuration
        );
    }

    private void OnDestroy()
    {
        m_PlayerStats.OnLevelUp -= OnLevelUp;
        EnemyStats.OnAnyEnemyDamaged -= HandleAnyEnemyDamaged;
    }
}
