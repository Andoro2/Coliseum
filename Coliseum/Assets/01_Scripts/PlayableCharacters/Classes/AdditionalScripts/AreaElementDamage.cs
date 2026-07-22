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

    public float m_Duration = 0f;
    public float m_EffectTickTimer = 1f;
    public bool m_DamageEffect = false, m_HealEffect = false;
    private float m_DeathTime;

    public float m_HealValue;

    public Dictionary<WorldElements, float> m_AttackElements = new Dictionary<WorldElements, float>();
    
    public void AddAutoAttackElement(WorldElements element, float percentage)
    {
        if (m_AttackElements.ContainsKey(element))
            m_AttackElements[element] = Mathf.Clamp01(m_AttackElements[element] + percentage);
        else
            m_AttackElements[element] = Mathf.Clamp01(percentage);
    }

    public HashSet<GameObject> m_ThingsInRange = new HashSet<GameObject>();

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

        if (m_Duration != 0f)
        {
            m_DeathTime = Time.time + m_Duration;
            if (m_DamageEffect) InvokeRepeating("DamageInArea", 0f, m_EffectTickTimer);
            if (m_HealEffect) InvokeRepeating("HealInArea", 0f, m_EffectTickTimer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > m_DeathTime)
        {
            Destroy(gameObject);
        }
    }
    private void DamageInArea()
    {
        m_ThingsInRange.Clear();

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
                if (m_DamageEffect)
                {
                    if (col.CompareTag("Enemy"))
                        m_ThingsInRange.Add(col.gameObject);
                }
                if (m_HealEffect)
                {
                    if (col.CompareTag("Player"))
                        m_ThingsInRange.Add(col.gameObject);
                }
            }
        }

        foreach (GameObject thing in m_ThingsInRange)
        {
            if (thing.CompareTag("Enemy"))
            {
                thing.GetComponentInParent<EnemyStats>().TakeDamageServerRpc(
                PS.m_Damage * PS.m_DamageMultiplier,
                BuildElementArray(),
                false,
                0f,
                EnemyStats.Killer.Player,
                NetworkManager.Singleton.LocalClientId
                );
            }
            if(thing.CompareTag("Player"))
            {
                thing.GetComponent<PlayerController>().HealServerRpc(m_HealValue);
            }
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
