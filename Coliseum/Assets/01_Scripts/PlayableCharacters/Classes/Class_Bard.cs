using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Rendering.FilterWindow;

public class Class_Bard : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Pasiva nivel 4:")]
    public float m_BonusCDPercent = 0.2f;

    [Header("Pasiva nivel 8:")]
    public float m_BonusResistPersonal = 0.1f;
    public float m_BonusResistAllies = 0.05f;
    public GameObject m_Aura;

    [Header("Pasiva nivel 12:")]
    [SerializeField] private AutoAttack AA;
    public float m_ElementBonus;
    public WorldElements elementoAleatorio = WorldElements.Null;
    private WorldElements[] m_WorldElementsArray;

    [Header("Pasiva nivel 16:")]
    public float m_AutoAttackReductor = 0.5f;
    public float m_TimeToRepeat = 10f;
    private float m_LastDoubleAttack;

    [Header("Pasiva nivel 20:")]
    public float m_LegendBuffDuration = 30f;
    private float m_BuffTimeStamp;
    public float m_BuffStatsPercentageElite = 5f;
    public float m_BuffStatsPercentageRoundBoss = 15f;
    public bool m_IsBuffed = false;
    private EnemyStats.EnemyClasses m_LastKillClass;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();
        AA = GetComponent<AutoAttack>();

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Bard);

        m_LastDoubleAttack = Time.time;

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_PassiveLevel20 && m_IsBuffed)
        {
            if(Time.time > (m_BuffTimeStamp + m_LegendBuffDuration))
            {
                m_IsBuffed = false;
                LegendBuff(false, m_LastKillClass);
            }
        }
    }
    public void ReduceDoubleAttackCooldown()
    {
        if (Time.time > (m_LastDoubleAttack + m_TimeToRepeat))
        {
            AA.BardDoubleHitSwitch();
            m_LastDoubleAttack = Time.time;
        }
        else
        {
            m_LastDoubleAttack -= m_AutoAttackReductor;
            if(AA.BardDoubleHitCheck()) AA.BardDoubleHitSwitch();
        }

    }
    private void BardRandomElement()
    {
        if (m_PassiveLevel12)
        {
            if (elementoAleatorio != WorldElements.Null)
                m_PlayerStats.RemoveAutoAttackElement(elementoAleatorio, m_ElementBonus);

            elementoAleatorio = m_WorldElementsArray[Random.Range(0, m_WorldElementsArray.Length)];
            m_PlayerStats.AddAutoAttackElement(elementoAleatorio, m_ElementBonus);
            //Debug.Log("bonus: "+ m_ElementBonus);
        }
    }
    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 4 && !m_PassiveLevel4)
        {
            m_PlayerStats.SetBonusCD(m_BonusCDPercent);

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            CounterEnchantment();

            m_PassiveLevel8 = true;
        }
        if (newLevel >= 12 && !m_PassiveLevel12)
        {
            System.Array elementsSystemArray = System.Enum.GetValues(typeof(WorldElements));

            // Filtramos para crear un array de WorldElements que NO incluya el Null
            m_WorldElementsArray = elementsSystemArray.Cast<WorldElements>()
                             .Where(e => e != WorldElements.Null)
                             .ToArray();

            if (AA != null) AA.OnAttack += BardRandomElement;

            m_PassiveLevel12 = true;
        }
        if (newLevel >= 16 && !m_PassiveLevel16)
        {
            if (AA != null) AA.OnAttack += ReduceDoubleAttackCooldown;

            m_PassiveLevel16 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    public void CounterEnchantment()
    {
        foreach (WorldElements damage in System.Enum.GetValues(typeof(WorldElements))) {
            if(damage != WorldElements.Null) m_PlayerStats.AddDamageResistancePermanent(damage, m_BonusResistPersonal);
        }
        m_Aura.SetActive(true);
        //ApplyGroupBonusServerRpc();
    }

    public void LegendBuff(bool activationSwitch, EnemyStats.EnemyClasses enemyClass)
    {
        if (m_LastKillClass == enemyClass) return; // clase dferente o no

        float buff = 0f;
        if (enemyClass == EnemyStats.EnemyClasses.Elite) buff = m_BuffStatsPercentageElite;
        if (enemyClass == EnemyStats.EnemyClasses.RoundBoss) buff = m_BuffStatsPercentageRoundBoss;

        if (activationSwitch && !m_IsBuffed) // se bufa
        {
            m_PlayerStats.ApplyDamageBonus(buff);
            m_PlayerStats.ApplyAttackSpeedBonus(buff);
            m_PlayerStats.ApplySpeedBonus(buff);
            m_PlayerStats.ApplyHealthBonus(buff);

            m_IsBuffed = true;
            m_BuffTimeStamp = Time.time;
        }
        else // se debufa
        {
            m_PlayerStats.ApplyDamageBonus(-buff);
            m_PlayerStats.ApplyAttackSpeedBonus(-buff);
            m_PlayerStats.ApplySpeedBonus(-buff);
            m_PlayerStats.ApplyHealthBonus(-buff);

            m_IsBuffed = false;
        }

        m_LastKillClass = enemyClass;
    }

    
    /*[ServerRpc(RequireOwnership = false)]
    private void ApplyGroupBonusServerRpc()
    {
        ApplyGroupBonusClientRpc();
    }

    [ClientRpc]
    private void ApplyGroupBonusClientRpc()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId) continue; // continue -> skip to next iteration

            foreach (WorldElements damage in System.Enum.GetValues(typeof(WorldElements)))
            {
                if (damage != WorldElements.Null) player.GetComponent<PlayerStats>().AddDamageResistancePermanent(damage, m_BonusResistAllies);
            }
        }
    }*/
}
