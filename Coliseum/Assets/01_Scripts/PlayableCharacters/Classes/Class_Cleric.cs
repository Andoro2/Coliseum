using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Class_Cleric : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Pasiva nivel 8:")]
    public float m_DropChance = 0.1f;

    [Header("Pasiva nivel 20:")]
    public bool m_ClericRevive = false;
    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Cleric);

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnLevelUp(int newLevel)
    {
        if (newLevel >= 4 && !m_PassiveLevel4)
        {
            GetComponentInChildren<AutoAttack>().OnAttack += SpawnRadiantArea;

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            EnemyStats.OnAnyEnemyDeath += OnEnemyDeath;

            m_PassiveLevel8 = true;
        }
        if (newLevel >= 12 && !m_PassiveLevel12)
        {
            m_PassiveLevel12 = true;
            m_DropChance += 0.1f;
        }
        if (newLevel >= 16 && !m_PassiveLevel16)
        {
            m_PassiveLevel16 = true;
            m_DropChance += 0.1f;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            ClericReliveSwitch();

            m_PassiveLevel20 = true;
            m_DropChance += 0.1f;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }

    // level 4
    private void SpawnRadiantArea()
    {
        Vector3 spawnPos = m_PlayerStats.transform.position + m_PlayerStats.transform.forward * 2f;
        m_PlayerStats.transform.parent.GetComponent<PlayerController>()
            .SpawnObjectServerRpc(spawnPos, PlayerStats.SpawnableObject.ClericAreaL4);
    }

    // level 8
    private void OnEnemyDeath(Vector3 position, EnemyStats.Killer source, ulong attackerClientId)
    {
        if (source != EnemyStats.Killer.Player) return;
        if (attackerClientId != m_PlayerStats.OwnerClientId) return;
        if (Random.value >= m_DropChance) return;

        m_PlayerStats.transform.parent.GetComponent<PlayerController>().SpawnObjectServerRpc(position, PlayerStats.SpawnableObject.ClericHealL8);
    }

    public void ClericReliveSwitch()
    {
        if (m_ClericRevive) m_ClericRevive = false;
        else m_ClericRevive = true;
    }

    private void OnDestroy()
    {
        EnemyStats.OnAnyEnemyDeath -= OnEnemyDeath;
    }
}
