using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class AutoAttack_Melee : AutoAttack
{
    public float m_ElementalPercent;
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
        /*
        // --- transformar el diccionario de autoAttackElements a un array --- //
        ElementDamage[] elements = new ElementDamage[PS.m_AutoAttackElements.Count + 1];

        int i = 0;
        elements[0] = new ElementDamage { Element = WorldElements.Null, Percentage = 1f };
       
        foreach (var kvp in PS.m_AutoAttackElements)
            elements[i++] = new ElementDamage { Element = kvp.Key, Percentage = kvp.Value };
        // --- //
        */
        bool isCrit = Random.value <= PS.m_CriticChance; // Random.value equivale al rango entre 0f y 1f

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            hit.GetComponentInParent<EnemyStats>().TakeDamage(
                PS.m_Damage * PS.m_DamageMultiplier,
                BuildElementArray(),
                isCrit,
                PS.m_CriticExtra,
                EnemyStats.Killer.Player
            );
            if (m_BardDoubleHit)
                Debug.Log("Bard double hit");
            hit.GetComponentInParent<EnemyStats>().TakeDamage(
                PS.m_Damage * PS.m_DamageMultiplier,
                BuildElementArray(),
                isCrit,
                PS.m_CriticExtra,
                EnemyStats.Killer.Player
            );
        }

        SpawnAreaDamage(PC.transform.position + PC.transform.forward * m_AttackRange * 0.5f, isCrit);

        if (m_VFX != null)
            Instantiate(m_VFX, PC.transform.position + PC.transform.forward, PC.transform.rotation);
    }
}