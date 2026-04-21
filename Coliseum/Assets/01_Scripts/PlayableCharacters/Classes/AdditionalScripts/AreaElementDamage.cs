using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AreaElementDamage : MonoBehaviour
{
    private PlayerStats PS;

    public float m_HexSize = 3f;
    public float m_DetectionHeight = 2f;
    private float HexWidth => m_HexSize * Mathf.Sqrt(3f) * 2f;
    private float HexHeight => m_HexSize * 2f;

    public Dictionary<WorldElements, float> m_AttackElements = new Dictionary<WorldElements, float>();
    public void AddAutoAttackElement(WorldElements element, float percentage)
    {
        if (m_AttackElements.ContainsKey(element))
            m_AttackElements[element] = Mathf.Clamp01(m_AttackElements[element] + percentage);
        else
            m_AttackElements[element] = Mathf.Clamp01(percentage);
    }

    public HashSet<GameObject> m_EnemiesInRange = new HashSet<GameObject>();

    public LayerMask m_EnemyLayer;
    private static readonly Quaternion[] boxRotations = new Quaternion[]
    {
        Quaternion.Euler(0,   0, 0),
        Quaternion.Euler(0,  60, 0),
        Quaternion.Euler(0, 120, 0)
    };

    // Start is called before the first frame update
    void Start()
    {
        PS = GameObject.FindWithTag("Player").gameObject.transform.GetComponent<PlayerStats>();

        DamageInArea();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void DamageInArea()
    {
        m_EnemiesInRange.Clear();

        Vector3 halfExtents = new Vector3(HexWidth / 2f, m_DetectionHeight / 2f, HexHeight / 2f);

        foreach (Quaternion rotation in boxRotations)
        {
            Collider[] hits = Physics.OverlapBox(
                transform.position,
                halfExtents,
                rotation,
                m_EnemyLayer
            );

            foreach (Collider col in hits)
            {
                if (col.CompareTag("Enemy"))
                    m_EnemiesInRange.Add(col.gameObject);
            }
        }

        foreach (GameObject enemy in m_EnemiesInRange)
        {
            if (!enemy.CompareTag("Enemy")) continue;
            enemy.GetComponentInParent<EnemyStats>().TakeDamageServerRpc(
                PS.m_Damage * PS.m_DamageMultiplier,
                BuildElementArray(),
                false,
                0f,
                EnemyStats.Killer.Player,
                NetworkManager.Singleton.LocalClientId
            );
        }
    }
    private ElementDamage[] BuildElementArray()
    {
        List<ElementDamage> elements = new List<ElementDamage>();
        elements.Add(new ElementDamage { Element = WorldElements.Null, Percentage = 1f });

        foreach (var kvp in m_AttackElements)
            elements.Add(new ElementDamage { Element = kvp.Key, Percentage = kvp.Value });

        return elements.ToArray();
    }
}
