using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItem : MonoBehaviour
{
    public float m_HealAmount;
    public bool m_IsPermanent;
    public float m_LifeTime;


    public GameObject m_Impact_VFX;
    void Start()
    {
        if (m_LifeTime > 0) Destroy(gameObject, m_LifeTime);
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().Heal(m_HealAmount);
            //if (m_Impact_VFX != null) SpawnVFX();
            Destroy(gameObject);
        }
    }

    private void SpawnVFX()
    {
        if (m_Impact_VFX != null)
            Instantiate(m_Impact_VFX, transform.position, Quaternion.identity);
    }
}
