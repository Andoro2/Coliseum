using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class HexAreaDamage : MonoBehaviour
{
    public bool m_AllyDamage = true; // true = daño a enemigos | false = daño a aliados

    [System.Serializable]
    public class AreaElement
    {
        public WorldElements Element;
        public float Percentage;
    }

    public List<AreaElement> m_Elements = new List<AreaElement>();
    public float m_DamagePercent = 0.3f;
    public float m_Lifetime = 0.1f;
    public float m_AreaSize = 1f;

    private float m_Damage;
    private bool m_IsCrit;
    private float m_CritExtra;
    private ulong m_AttackerClientId;

    public void Initialize(float damage, bool isCrit, float critExtra, ulong attackerClientId)
    {
        m_Damage = damage * m_DamagePercent;
        m_IsCrit = isCrit;
        m_CritExtra = critExtra;
        m_AttackerClientId = attackerClientId;

        ApplyHexDamage();

        Destroy(gameObject, m_Lifetime);
    }

    private void ApplyHexDamage()
    {
        float hexWidth = m_AreaSize * Mathf.Sqrt(3) * 2f;
        float hexHeight = m_AreaSize * 2f;
        HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 60f * i + 30f, 0);
            Vector3 halfExtents = new Vector3(hexWidth * 0.5f, 1f, hexHeight * 0.5f);
            Collider[] colliders = Physics.OverlapBox(transform.position, halfExtents, rotation);

            foreach (Collider col in colliders)
            {
                string tag = m_AllyDamage ? "Enemy" : "Player"; // si es daño aliado, tag = enemy, si es daño enemigo, tag = player

                if (!col.CompareTag(tag)) continue;
                if (alreadyHit.Contains(col.gameObject)) continue;

                alreadyHit.Add(col.gameObject);

                ElementDamage[] elements = new ElementDamage[m_Elements.Count];
                for (int j = 0; j < m_Elements.Count; j++)
                    elements[j] = new ElementDamage { Element = m_Elements[j].Element, Percentage = m_Elements[j].Percentage };

                if (m_AllyDamage)
                {
                    col.GetComponentInParent<EnemyStats>().TakeDamageServerRpc(
                        m_Damage,
                        elements,
                        m_IsCrit,
                        m_CritExtra,
                        EnemyStats.Killer.Player,
                        m_AttackerClientId
                    );
                }
                else
                {
                    col.GetComponentInParent<PlayerStats>().TakeDamageServerRpc(
                        m_Damage,
                        elements,
                        m_IsCrit,
                        m_CritExtra,
                        m_AttackerClientId
                    );
                }
            }
        }
    }
}
