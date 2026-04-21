using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoAttack_Ranged : AutoAttack
{
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

        bool isCrit = Random.value <= PS.m_CriticChance;

        GameObject projectile = Instantiate(m_ProjectilePrefab, transform.position, transform.rotation);
        //projectile.GetComponent<ProjectileForward>().target = m_Target;
        // Initialize y pasar info al proyectil
        projectile.GetComponent<PlayerGenericProjectile>().ProjectileData(
        m_Target,
        PS.m_Damage * PS.m_DamageMultiplier,
        BuildElementArray(),
        isCrit,
        PS.m_CriticExtra,
        NetworkManager.Singleton.LocalClientId
        );

        if (m_BardDoubleHit) projectile.GetComponent<PlayerGenericProjectile>().BardDoubleHit();
    }

    //[ServerRpc(RequireOwnership = false)]]
}
