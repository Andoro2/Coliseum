using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class ProjectileForward : MonoBehaviour
{
    public GameObject target;
    public float speed = 20f,
        m_Damage,
        m_ElementalPercentage;
    public WorldElements m_ProjectileElement;
    //public EnemySpawner.Types m_Element = EnemySpawner.Types.Normal;

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
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyManager>().TakeDamage(m_Damage, m_ElementalPercentage, m_ProjectileElement);
            Destroy(gameObject);
        }
    }
}
