using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealItem : NetworkBehaviour
{
    public float m_HealAmount;



    public GameObject m_Impact_VFX;
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // solo el servidor gestiona la colisión

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().Heal(m_HealAmount);
            //if (m_Impact_VFX != null) SpawnVFXClientRpc();
            GetComponentInParent<NetworkObject>().Despawn();
        }
    }

    [ClientRpc]
    private void SpawnVFXClientRpc()
    {
        if (m_Impact_VFX != null)
            Instantiate(m_Impact_VFX, transform.position, Quaternion.identity);
    }
}
