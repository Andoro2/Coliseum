using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class ProjectileTower : MonoBehaviour
{
    private TurretStatsSO m_TurretStats;

    public float m_Damage, m_ElementalPercentage;
    //public EnemySpawner.Types m_ProjectileElement = EnemySpawner.Types.Normal;
    //public float m_ShootPerMinute = 30f, m_Range = 1000f;
    private int m_Level;

    public float m_ShootTimer = 0f;
    public GameObject m_Projectile,
        m_ShootPoint;

    public GameObject m_Target;

    public List<GameObject> m_EnemiesInRange = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        m_TurretStats = GetComponent<TowerStats>().m_TurretStats;
        m_Damage = m_TurretStats.m_Damage;
        //m_ElementalPercentage = m_TurretStats.m_ElementalDamagePercentage;
        m_ShootTimer = 60f / m_TurretStats.m_ShootsPerMinute;
        //m_Level = GetComponent<TowerStats>().m_Level;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_Level!= GetComponent<TowerStats>().m_Level)
        {
            m_Level = GetComponent<TowerStats>().m_Level;
            IncreaseDamage();
        }

        if (m_ShootTimer > 0) m_ShootTimer -= Time.deltaTime;

        UpdateEnemyList();
        if(m_Target == null) m_Target = SetTarget();

        if (m_Target != null)
        {
            m_ShootPoint.transform.LookAt(m_Target.transform);

            if (m_ShootTimer <= 0)
            {
                Shoot();
                m_ShootTimer = 60f / m_TurretStats.m_ShootsPerMinute; ;
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
        //projectile.GetComponent<ProjectileForward>().m_ElementalOercentage = m_ElementalPercentage;
        projectile.GetComponent<ProjectileForward>().m_ElementalPercentage = m_TurretStats.m_ElementPercentage;
    }
    public GameObject SetTarget()
    {
        if(m_EnemiesInRange.Count > 0)
        {
            m_EnemiesInRange.Sort((enemy1, enemy2) =>
            {
                if(enemy1 != null && enemy2 != null)
                {
                    float distanceToEnemy1 = Vector3.Distance(transform.position, enemy1.transform.position);
                    float distanceToEnemy2 = Vector3.Distance(transform.position, enemy2.transform.position);
                    return distanceToEnemy1.CompareTo(distanceToEnemy2);
                }
                else
                {
                    return 0;
                }
            });
            return m_EnemiesInRange[0];
        }
        else
        {
            return null;
        }

    }
    public void IncreaseDamage()
    {
        m_Damage = m_TurretStats.m_Damage * m_Level;
    }
    void UpdateEnemyList()
    {
        m_EnemiesInRange = GetComponentInChildren<InRangeManager>().enemiesInRange;
    }
}
