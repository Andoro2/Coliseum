using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoAttack_Ranged : AutoAttack
{
    public float m_Damage;

    public GameObject m_ProjectilePrefab;
   
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Attack()
    {
        if (m_ProjectilePrefab == null) return;

        m_Anim.SetTrigger("AttackRanged");

        GameObject projectile = Instantiate(m_ProjectilePrefab, transform.position, transform.rotation);
        projectile.GetComponent<ProjectileForward>().target = m_Target;
        // Initialize y pasar info al proyectil
        /*
         * projectile.GetComponent<ProjectileForward>().Initialize(
         * m_Target,
         * m_Damage * PS.m_DamageMultiplier,
         * m_ElementalPercent,
         * m_Element,
         * NetworkManager.Singleton.LocalClientId
         * );
         */
    }
}
