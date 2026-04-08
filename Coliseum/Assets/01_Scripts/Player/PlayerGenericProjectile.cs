using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerGenericProjectile : MonoBehaviour
{
    public float m_Damage, m_ElementalPercent, m_AttackRange, m_Speed = 5f;
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

        foreach (ElementDamage item in elements)
        {
            ListaDeElementos.Clear();
            ListaDeElementos.Add(item.Element);
        }
    }
    public void ProjectileData(GameObject target, float damage, Dictionary<WorldElements, float> attackElements, ulong attackerClientId)
    {
        m_Target = target;
        m_Damage = damage;

        // --- transformar el diccionario de autoAttackElements a un array para que el Netcode pueda enviarlo --- //
        elements = new ElementDamage[attackElements.Count];
        int i = 0;
        foreach (var kvp in attackElements)
            elements[i++] = new ElementDamage { Element = kvp.Key, Percentage = kvp.Value };
        // --- //

        // m_AttackElements = attackElements;
        m_PlayerNetworkID = attackerClientId;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, m_AttackRange * 0.5f);

            foreach (Collider hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;
                hit.GetComponent<EnemyStats>().TakeDamageServerRpc(
                m_Damage,
                elements,
                EnemyStats.Killer.Player,
                m_PlayerNetworkID
                );
            }

            if(m_Impact_VFX != null) Instantiate(m_Impact_VFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
