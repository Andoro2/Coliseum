using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectileForward : MonoBehaviour
{
    public TowerStats m_OwnerTower;
    public GameObject target;
    public float speed = 20f,
        m_Damage;
    //public WorldElements m_ProjectileElement;

    private ElementDamage[] elements;
    public Dictionary<WorldElements, float> m_AttackElements = new Dictionary<WorldElements, float>();

    private void Start()
    {
        
    }
    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target.transform);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
        else
        {         
            Destroy(gameObject);
        }

        if(transform.position.y < 0 || transform.position.y > 10)
        {
            Destroy(gameObject);
        }
    }
    public void Initialize(float damage, ElementDamage[] attackElements, TowerStats towerSource = null, PlayerStats playerSource = null)
    {
        m_Damage = damage;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyStats>().TakeDamage(m_Damage, elements, false, 0f, null, m_OwnerTower);
            Destroy(gameObject);
        }
    }
}
