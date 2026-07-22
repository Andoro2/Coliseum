using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HexAreaDamageOnContact : NetworkBehaviour
{
    [System.Serializable]
    public class AreaElement
    {
        public WorldElements Element;
        public float Percentage;
    }

    public List<AreaElement> m_Elements = new List<AreaElement>();
    public float m_Damage = 50f;

    public bool m_Permanent = false;
    public float m_LifeTime;

    public void Start()
    {
        Debug.Log($"HexAreaDamage Start. IsServer: {IsServer}, IsSpawned: {IsSpawned}");

        if (!IsServer) return;
        if (m_LifeTime <= 0f) m_LifeTime = 0.5f;
        if (!m_Permanent) Invoke(nameof(Despawn), m_LifeTime);
    }
    public void Despawn()
    {
        if (!IsServer) return;
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter: {other.name}, IsServer: {IsServer}");

        if (!IsServer) return; // solo el servidor gestiona la colisión

        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (pc == null) return;

            ElementDamage[] elements = new ElementDamage[m_Elements.Count];
            for (int j = 0; j < m_Elements.Count; j++)
                elements[j] = new ElementDamage { Element = m_Elements[j].Element, Percentage = m_Elements[j].Percentage };


            pc.TakeDamageServerRpc(
                m_Damage,
                elements,
                false,
                1.5f
            );
        }
    }
}
