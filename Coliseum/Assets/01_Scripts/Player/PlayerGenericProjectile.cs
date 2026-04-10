using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerGenericProjectile : MonoBehaviour
{
    public float m_Damage, m_ElementalPercent, m_AttackRange, m_Speed = 5f;
    public bool m_IsCrit = false;
    public float m_CritExtra = 1.5f;
    public GameObject m_Target;
    // private Dictionary<WorldElements, float> m_AttackElements;
    public ElementDamage[] elements;
    public List<WorldElements> ListaDeElementos = new List<WorldElements>();
    public ulong m_PlayerNetworkID;
    public GameObject m_Impact_VFX;

    // Update is called once per frame
    void Update()
    {
        if (m_Target != null)
        {
            transform.LookAt(m_Target.transform);
            transform.position = Vector3.MoveTowards(transform.position, m_Target.transform.position, m_Speed * Time.deltaTime);
        }
        else transform.position += transform.forward * m_Speed * Time.deltaTime;
    }
    public void ProjectileData(GameObject target, float damage, ElementDamage[] attackElements, bool isCrit, float critExtra, ulong attackerClientId)
    {
        ListaDeElementos.Clear();

        m_Target = target;
        m_Damage = damage;
        m_PlayerNetworkID = attackerClientId;
        /*
        // --- transformar el diccionario de autoAttackElements a un array para que el Netcode pueda enviarlo --- //
        elements = new ElementDamage[attackElements.Length + 1];

        elements[0] = new ElementDamage { Element = WorldElements.Null, Percentage = 1f };
        ListaDeElementos.Add(elements[0].Element);

        int i = 1;
        foreach (ElementDamage element in attackElements)
        {
            elements[i++] = new ElementDamage { Element = element.Element, Percentage = element.Percentage };
            if (!ListaDeElementos.Contains(element.Element))
                ListaDeElementos.Add(element.Element);
        }
        // --- //
        */

        elements = attackElements;
        foreach (ElementDamage element in attackElements)
        {
            if (!ListaDeElementos.Contains(element.Element))
                ListaDeElementos.Add(element.Element);
        }

        if (isCrit)
        {
            m_IsCrit = isCrit;
            m_CritExtra = critExtra;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            //Collider[] hits = Physics.OverlapSphere(transform.position, m_AttackRange * 0.5f);

            // foreach (Collider hit in hits)            {
            other.GetComponent<EnemyStats>().TakeDamageServerRpc(
                m_Damage,
                elements,
                m_IsCrit,
                m_CritExtra,
                EnemyStats.Killer.Player,
                m_PlayerNetworkID
                );
            //}

            if(m_Impact_VFX != null) Instantiate(m_Impact_VFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
