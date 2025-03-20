using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballista : MonoBehaviour
{
    public float m_Damage,
        m_ShootRate,
        m_Range = 25f;

    private float m_ShootTimer;
    private BoxCollider m_Collider;
    public GameObject m_Projectile,
        m_ShootPoint;

    public GameObject m_Target;

    public List<GameObject> m_EnemiesInRange = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<BoxCollider>();
        m_Collider.size = new Vector3(m_Range, 1f, m_Range);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_ShootTimer > 0) m_ShootTimer -= Time.deltaTime;
        else
        {
            Shoot();
            m_ShootTimer = m_ShootRate;
        }
        m_Target = SetTarget();
        if(SetTarget() != null)
        {
            m_ShootPoint.transform.LookAt(SetTarget().transform);

        }
    }
    void Shoot()
    {
        Debug.Log("Disparo");
        //Instantiate(m_Projectile);
    }
    GameObject SetTarget()
    {
        if(m_EnemiesInRange.Count > 0)
        {
            m_EnemiesInRange.Sort((enemy1, enemy2) =>
            {
                float distanceToEnemy1 = Vector3.Distance(transform.position, enemy1.transform.position);
                float distanceToEnemy2 = Vector3.Distance(transform.position, enemy2.transform.position);
                return distanceToEnemy1.CompareTo(distanceToEnemy2);
            });
            return m_EnemiesInRange[0];
        }
        else
        {
            return null;
        }

    }
    public void EnemyEntersRange(GameObject enemy)
    {
        if (!m_EnemiesInRange.Contains(enemy)) m_EnemiesInRange.Add(enemy);
    }
    public void EnemyLeavesRange(GameObject enemy)
    {
        if (m_EnemiesInRange.Contains(enemy)) m_EnemiesInRange.Remove(enemy);
    }
    public void IncreaseRange(float ExtraRange)
    {
        m_Range += ExtraRange;
        m_Collider.size = new Vector3(m_Range, 1f, m_Range);
    }
    public void IncreaseDamage(float ExtraDamage)
    {
        m_Damage+= ExtraDamage;
    }
}
