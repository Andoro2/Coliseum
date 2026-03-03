using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTower : MonoBehaviour
{
    private TurretStatsSO m_TurretStats;

    public float m_Damage, m_ElementalPercentage;
    private int m_Level;

    public float m_ShootTimer = 0f;
    public GameObject m_Projectile, m_ShootPoint;
    public GameObject m_Target;

    void Start()
    {
        m_TurretStats = GetComponent<TowerStats>().m_TurretStats;
        m_Damage = m_TurretStats.m_Damage;
        m_ShootTimer = 60f / m_TurretStats.m_ShootsPerMinute;
    }

    void Update()
    {
        if (m_Level != GetComponent<TowerStats>().m_Level)
        {
            m_Level = GetComponent<TowerStats>().m_Level;
            IncreaseDamage();
        }

        if (m_ShootTimer > 0) m_ShootTimer -= Time.deltaTime;

        m_Target = GetComponentInChildren<InRangeManager>().GetPriorityTarget();

        if (m_Target != null)
        {
            m_ShootPoint.transform.LookAt(m_Target.transform);

            if (m_ShootTimer <= 0)
            {
                Shoot();
                m_ShootTimer = 60f / m_TurretStats.m_ShootsPerMinute;
            }
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(m_Projectile, m_ShootPoint.transform);
        projectile.transform.SetParent(null);

        if (m_Target != null)
            projectile.GetComponent<ProjectileForward>().target = m_Target;

        projectile.GetComponent<ProjectileForward>().m_Damage = m_Damage;
        projectile.GetComponent<ProjectileForward>().m_ElementalPercentage = m_TurretStats.m_ElementPercentage;
    }

    public void IncreaseDamage()
    {
        m_Damage = m_TurretStats.m_Damage * m_Level;
    }
}