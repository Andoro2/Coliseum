using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttack_Ranged : AutoAttack
{
    public GameObject m_ProjectilePrefab;
    private bool isCrit = false;
    private GameObject m_AttackTarget;

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

        m_AttackTarget = m_Target;
        isCrit = Random.value <= PS.m_CriticChance;
    }

    public void ReleaseProjectile()
    {
        GameObject projectile = Instantiate(m_ProjectilePrefab, transform.position, transform.rotation);
        //projectile.GetComponent<ProjectileForward>().target = m_Target;
        // Initialize y pasar info al proyectil
        projectile.GetComponent<PlayerGenericProjectile>().ProjectileData(
        m_Target,
        PS.m_Damage * PS.m_DamageMultiplier,
        BuildElementArray(),
        isCrit,
        PS.m_CriticExtra,
        PS.m_AreaAttackPrefabs,
        PS.m_DamageMultiplier
        );

        if (m_BardDoubleHit) projectile.GetComponent<PlayerGenericProjectile>().BardDoubleHit();
    }

}