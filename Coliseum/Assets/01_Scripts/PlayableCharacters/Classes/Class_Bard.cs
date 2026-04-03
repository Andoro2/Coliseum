using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Class_Bard : NetworkBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Pasiva nivel 4:")]
    public float m_BonusCDPercent = 0.2f;

    [Header("Pasiva nivel 8:")]
    public float m_BonusResistPersonal = 0.1f;
    public float m_BonusResistAllies = 0.05f;
    public GameObject m_Aura;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Bard);

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
            m_PlayerStats.SetBonusCD(m_BonusCDPercent);

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            CounterEnchantment();

            m_PassiveLevel8 = true;
        }
        if (newLevel >= 12 && !m_PassiveLevel12)
        {
            m_PassiveLevel12 = true;
        }
        if (newLevel >= 16 && !m_PassiveLevel16)
        {
            m_PassiveLevel16 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    public void CounterEnchantment()
    {
        foreach (WorldElements damage in System.Enum.GetValues(typeof(WorldElements))) {
            if(damage != WorldElements.Null) m_PlayerStats.AddDamageResistancePermanent(damage, m_BonusResistPersonal);
        }
        m_Aura.SetActive(true);
        //ApplyGroupBonusServerRpc();
    }
    /*[ServerRpc(RequireOwnership = false)]
    private void ApplyGroupBonusServerRpc()
    {
        ApplyGroupBonusClientRpc();
    }

    [ClientRpc]
    private void ApplyGroupBonusClientRpc()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId) continue; // continue -> skip to next iteration

            foreach (WorldElements damage in System.Enum.GetValues(typeof(WorldElements)))
            {
                if (damage != WorldElements.Null) player.GetComponent<PlayerStats>().AddDamageResistancePermanent(damage, m_BonusResistAllies);
            }
        }
    }*/
}
