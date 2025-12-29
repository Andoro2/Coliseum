using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class EnemyManager : MonoBehaviour
{
    public float m_Health = 5f;
    public Slider m_HealthSlider;
    //public int m_Value = 50, m_ExpValue = 10;
    //public EnemySpawner.Types EnemyType = EnemySpawner.Types.Normal;

    public List<State> m_States = new List<State>();

    public EnemyStatsSO m_EnemyStats;
    // Start is called before the first frame update
    void Start()
    {
        m_Health = m_EnemyStats.m_Health;
        m_HealthSlider.maxValue = m_EnemyStats.m_Health;
        m_HealthSlider.value = m_EnemyStats.m_Health;
    }

    // Update is called once per frame
    void Update()
    {
        m_HealthSlider.value = m_Health;

        if(m_Health <= 0)
        {
            Death();
        }
    }
    
    public void Death()
    {
        //score
        foreach (GameObject turret in GetComponent<EnemyMovement>().m_TurretsTargetedBy)
        {
            if (turret.GetComponent<InRangeManager>().enemiesInRange.Contains(gameObject))
            {
                turret.GetComponent<InRangeManager>().RemoveFromList(gameObject);
            }
        }

        GameObject.FindWithTag("GameController").transform.GetComponent<GameManager>().GetPaid(m_EnemyStats.m_Reward);

        GameObject.FindWithTag("Player").GetComponent<PlayerController>().ObtainExp(m_EnemyStats.m_Experience);

        Destroy(gameObject);
    }
    public void TakeDamage(float Damage, float ElementalPercentage, WorldElements Elemento)
    {
        float AttackDamage = Damage + Damage * ElementalPercentage;

        foreach (EnemyStatsSO.ElementResistance Resist in m_EnemyStats.m_Resistancies)
        {
            if (Resist.Element == Elemento) AttackDamage = AttackDamage * ((100 - Resist.Resistance)/100);
        }
        /*switch (Elemento)
        {
            case WorldElements.Fire:
                AttackDamage = AttackDamage * (1 - m_EnemyStats.m_Resistancies);
                break;
            case WorldElements.Ice:
                AttackDamage = AttackDamage * (1 - m_ResistanceIce);
                break;
            case WorldElements.Wind:
                AttackDamage = AttackDamage * (1 - m_ResistanceWind);
                break;
            case WorldElements.Lightning:
                AttackDamage = AttackDamage * (1 - m_ResistanceLightning);
                break;
            case WorldElements.Tech:
                AttackDamage = AttackDamage * (1 - m_ResistanceTech);
                break;
            case WorldElements.Physical:
                AttackDamage = AttackDamage * (1 - m_ResistancePhysical);
                break;
            default:
                break;
        }*/

        m_Health -= AttackDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
    public class State
    {
        States ActiveState;
        float m_Intensity,
            m_ActiveTime;
    }
    public enum States
    {
        Null,
        Slow,
        Stun,
        Burn,
        Wet,
        Bleeding
    }
}
