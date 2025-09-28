using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class EnemyManager : MonoBehaviour
{
    public float m_Health = 5f;
    public Slider m_HealthSlider;
    public int m_Value = 50, m_ExpValue = 10;
    public EnemySpawner.Types EnemyType = EnemySpawner.Types.Normal;
    public float m_ResistanceFire,
        m_ResistanceIce,
        m_ResistanceWind,
        m_ResistanceLightning,
        m_ResistanceTech,
        m_ResistanceEarth,
        m_ResistancePhysical;

    public List<State> m_States = new List<State>();
    // Start is called before the first frame update
    void Start()
    {
        m_HealthSlider.maxValue = m_Health;
        m_HealthSlider.value = m_Health;
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
    public class State
    {
        EnemySpawner.States ActiveState;
        float m_Intensity,
            m_ActiveTime;
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

        GameObject.FindWithTag("GameController").transform.GetComponent<GameManager>().GetPaid(m_Value);

        GameObject.FindWithTag("Player").GetComponent<PlayerController>().ObtainExp(m_ExpValue);

        Destroy(gameObject);
    }
    public void TakeDamage(float Damage, float ElementalPercentage, WorldElements Elemento)
    {
        float AttackDamage = Damage + Damage * ElementalPercentage;

        switch (Elemento)
        {
            case WorldElements.Fire:
                AttackDamage = AttackDamage * (1 - m_ResistanceFire);
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
        }

        m_Health -= AttackDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

}
