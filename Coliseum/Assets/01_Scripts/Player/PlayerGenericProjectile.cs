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

    private List<GameObject> m_AreaAttackPrefabs;
    private float m_DamageMultiplier;

    public ulong m_PlayerNetworkID;
    public GameObject m_Impact_VFX;

    private bool m_BardDoubleHit = false;

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
    public void ProjectileData(GameObject target, float damage, ElementDamage[] attackElements, bool isCrit, float critExtra, List<GameObject> areaPrefabs, float damageMultiplier, ulong attackerClientId)
    {
        ListaDeElementos.Clear();

        m_Target = target;
        m_Damage = damage;
        m_PlayerNetworkID = attackerClientId;

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

        m_AreaAttackPrefabs = areaPrefabs;
        m_DamageMultiplier = damageMultiplier;
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

            if (m_BardDoubleHit)
            {
                Debug.Log("Bard double hit");
                other.GetComponent<EnemyStats>().TakeDamageServerRpc(
                    m_Damage,
                    elements,
                    m_IsCrit,
                    m_CritExtra,
                    EnemyStats.Killer.Player,
                    m_PlayerNetworkID
                );
            }

            foreach (GameObject areaPrefab in m_AreaAttackPrefabs)
            {
                if (areaPrefab == null) continue;
                GameObject area = Instantiate(areaPrefab, transform.position, Quaternion.identity);
                area.GetComponent<HexAreaDamage>().Initialize(
                    m_Damage,
                    m_IsCrit,
                    m_CritExtra,
                    m_PlayerNetworkID
                );
            }

            if (m_Impact_VFX != null) Instantiate(m_Impact_VFX, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
    public void BardDoubleHit()
    {
        m_BardDoubleHit = true;
    }
}
