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

        // --- transformar el diccionario de autoAttackElements a un array para que el Netcode pueda enviarlo --- //
        ElementDamage[] elements = new ElementDamage[PS.m_AutoAttackElements.Count];
        int i = 0;
        foreach (var kvp in PS.m_AutoAttackElements)
            elements[i++] = new ElementDamage { Element = kvp.Key, Percentage = kvp.Value };
        // --- //

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            hit.GetComponent<EnemyStats>().TakeDamageServerRpc(
            PS.m_Damage * PS.m_DamageMultiplier,
            elements,
            EnemyStats.Killer.Player,
            NetworkManager.Singleton.LocalClientId
            );
        }

        if(m_VFX != null)
            Instantiate(m_VFX, PC.transform.position + PC.transform.forward, PC.transform.rotation);
    }
}
