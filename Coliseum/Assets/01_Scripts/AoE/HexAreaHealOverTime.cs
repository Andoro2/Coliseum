using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HexAreaHealOverTime : MonoBehaviour
{
    public bool m_HealAllies = true; // true = heal allies | false = heal enemies

    public float m_HealPercent = 0.05f;

    public float m_Lifetime = 5f,
        m_CurationInterval = 0.5f,
        m_AreaSize = 1f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("HealArea", 0f, m_CurationInterval);
        Invoke("Despawn", m_Lifetime);
    }

    public void HealArea()
    {
        float hexWidth = m_AreaSize * Mathf.Sqrt(3) * 2f;
        float hexHeight = m_AreaSize * 2f;

        for (int i = 0; i < 3; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 60f * i + 30f, 0);
            Vector3 halfExtents = new Vector3(hexWidth * 0.5f, 1f, hexHeight * 0.5f);
            Collider[] colliders = Physics.OverlapBox(transform.position, halfExtents, rotation);

            foreach (Collider col in colliders)
            {
                string tag = m_HealAllies ? "Enemy" : "Player"; // si es daño aliado, tag = enemy, si es daño enemigo, tag = player

                if (!col.CompareTag(tag)) continue;

                if (m_HealAllies)
                {
                    col.GetComponentInParent<PlayerStats>().HealServerRpc(
                        col.GetComponentInParent<PlayerStats>().m_MaxHealth * m_HealPercent
                    );
                }
                else
                {
                    /*
                    col.GetComponentInParent<EnemyStats>().HealServerRpc(
                        m_Damage,
                        elements,
                        m_IsCrit,
                        m_CritExtra,
                        m_AttackerClientId
                    );
                    */
                }
            }
        }
    }

    public void Despawn()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
