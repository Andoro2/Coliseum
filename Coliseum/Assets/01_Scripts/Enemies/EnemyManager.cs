using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public float m_Health = 5f;
    public EnemySpawner.Types EnemyType = EnemySpawner.Types.Normal;
    public float m_ResistanceFire,
        m_ResistanceIce,
        m_ResistanceWind,
        m_ResistanceLightning,
        m_ResistanceTech,
        m_ResistanceEarth,
        m_ResistanceBlood;

    public List<State> m_States = new List<State>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
        Destroy(gameObject);
    }
    public void TakeDamage(float Damage, float ElementalDamage, EnemySpawner.Types Elemento)
    {
        m_Health -= Damage;

        switch (Elemento)
        {
            case EnemySpawner.Types.Fire:
                m_Health -= ElementalDamage * m_ResistanceFire;
                break;
            case EnemySpawner.Types.Ice:
                m_Health -= ElementalDamage * m_ResistanceIce;
                break;
            case EnemySpawner.Types.Wind:
                m_Health -= ElementalDamage * m_ResistanceWind;
                break;
            case EnemySpawner.Types.Lightning:
                m_Health -= ElementalDamage * m_ResistanceLightning;
                break;
            case EnemySpawner.Types.Earth:
                m_Health -= ElementalDamage * m_ResistanceEarth;
                break;
            case EnemySpawner.Types.Tech:
                m_Health -= ElementalDamage * m_ResistanceTech;
                break;
            case EnemySpawner.Types.Blood:
                m_Health -= ElementalDamage * m_ResistanceBlood;
                break;
        }
    }

}
