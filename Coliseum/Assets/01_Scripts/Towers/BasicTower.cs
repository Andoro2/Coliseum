using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTower : MonoBehaviour
{
    private TowerStats m_TowerStats;

    public float m_ShootTimer = 0f;
    public GameObject m_Projectile, m_ShootPoint;
    public GameObject m_Target;


    void Start()
    {
        m_TowerStats = GetComponent<TowerStats>();
        m_ShootTimer = 60f / m_TowerStats.m_Cadency;
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
                m_ShootTimer = 60f / m_TowerStats.m_Cadency;
            }
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(m_Projectile, m_ShootPoint.transform);
        projectile.transform.SetParent(null);

        ProjectileForward pf = projectile.GetComponent<ProjectileForward>();

        if (m_Target != null)
            pf.target = m_Target;

        pf.m_Damage = m_TowerStats.m_Damage;
        pf.m_ElementalPercentage = m_TowerStats.m_ElementPercentage;
        pf.m_OwnerTower = m_TowerStats;
    }
}