using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAttack_Melee : AutoAttack
{
    public float m_Damage,
        m_ElementalPercent;
    private WorldElements m_Element;
    public GameObject m_VFX;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

    }

    // Update is called once per frame
    protected override void Attack()
    {
        m_Anim.SetTrigger("AttackMelee");
        Collider[] hits = Physics.OverlapSphere(PC.transform.position + PC.transform.forward * (m_AttackRange * 0.5f), m_AttackRange * 0.5f);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            hit.GetComponent<EnemyManager>().TakeDamage(m_Damage * PS.m_DamageMultiplier, m_ElementalPercent, m_Element);
        }

        if(m_VFX != null) Instantiate(m_VFX, PC.transform.position + PC.transform.forward, PC.transform.rotation);
    }
}
