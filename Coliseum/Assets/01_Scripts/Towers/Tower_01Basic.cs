using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_01Basic : MonoBehaviour
{
    private TowerStats m_TowerStats;

    public float m_ShootTimer;
    public GameObject m_Projectile, m_ShootPoint;
    public GameObject m_Target;


    void Start()
    {
        m_TowerStats = GetComponent<TowerStats>();
        m_ShootTimer = 60 / m_TowerStats.m_ShootsPerMinute;
    }

    void Update()
    {
        if (m_ShootTimer > 0) m_ShootTimer -= Time.deltaTime;

        m_Target = GetComponentInChildren<InRangeManager>().GetPriorityTarget();

        if (m_Target != null)
        {
            m_ShootPoint.transform.LookAt(m_Target.transform);

            if (m_ShootTimer <= 0)
            {
                Shoot();
                m_ShootTimer = 60f / m_TowerStats.m_ShootsPerMinute;
            }
        }
    }

    void Shoot()
    {
        Debug.Log("Shoot");
        GameObject projectile = Instantiate(m_Projectile, m_ShootPoint.transform);
        projectile.transform.SetParent(null);

        TurretProjectileForward pf = projectile.GetComponent<TurretProjectileForward>();

        if (m_Target != null)
            pf.target = m_Target;

        pf.Initialize(m_TowerStats.m_Damage, BuildElementArray(), m_TowerStats, null);
    }

    protected ElementDamage[] BuildElementArray()
    {
        List<ElementDamage> elements = new List<ElementDamage>();
        elements.Add(new ElementDamage { Element = WorldElements.Null, Percentage = 1f });

        foreach (var kvp in m_TowerStats.m_AttackElements)
            elements.Add(new ElementDamage { Element = kvp.Key, Percentage = kvp.Value });

        return elements.ToArray();
    }
}