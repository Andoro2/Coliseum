using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Class_Fighter : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel4_10 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    // level 4
    private AutoAttack_Melee m_AutoAttackMelee;
    private AutoAttack_Ranged m_AutoAttackRanged;

    [Header("Level 16")]
    public HashSet<GameObject> m_EnemiesInRange = new HashSet<GameObject>();
    public float m_EnemyDetectionRange = 7.5f;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        //GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Fighter);

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;

        m_AutoAttackMelee = GetComponent<AutoAttack_Melee>();
        m_AutoAttackRanged = GetComponent<AutoAttack_Ranged>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_PassiveLevel12) PassiveLevel12();

    }
    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 4 && !m_PassiveLevel4)
        {
            if (m_AutoAttackMelee != null) m_AutoAttackMelee.m_FighterChainAttacks = true;
            if (m_AutoAttackRanged != null) m_AutoAttackRanged.m_FighterChainAttacks = true;

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            m_PlayerStats.SetImmunity(StatusEffect.Fear, true);
            m_PlayerStats.SetImmunity(StatusEffect.Stun, true);

            m_PassiveLevel8 = true;
        }
        if(newLevel >= 10 && !m_PassiveLevel4_10)
        {
            if (m_AutoAttackMelee != null) m_AutoAttackMelee.SetDoubleAttackFrequency(2);
            if (m_AutoAttackRanged != null) m_AutoAttackRanged.SetDoubleAttackFrequency(2);
            m_PassiveLevel4_10 = true;
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
            if (m_AutoAttackMelee != null) m_AutoAttackMelee.SetDoubleAttackFrequency(1);
            if (m_AutoAttackRanged != null) m_AutoAttackRanged.SetDoubleAttackFrequency(1);

            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }

    private void PassiveLevel12()
    {
        float armorBonus = 0f;

        Collider[] closeEnemies = Physics.OverlapSphere(transform.position, m_EnemyDetectionRange * 0.5f);

        foreach (Collider hit in closeEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                m_EnemiesInRange.Add(hit.gameObject);
            }

            armorBonus = Mathf.Min(50f, m_EnemiesInRange.Count * 0.05f);
        }

        m_PlayerStats.ApplyArmorPercentageEffect(PlayerArmorEffectSource.FighterLevel12, armorBonus, 5f);

        m_EnemiesInRange.Clear();
    }
}
