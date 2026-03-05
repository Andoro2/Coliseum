using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC5_Gauthak : MonoBehaviour
{
    private PlayerStats m_PlayerStats;
    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        // Suscribirse al evento de subida de nivel
        m_PlayerStats.OnLevelUp += OnLevelUp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLevelUp(int newLevel)
    {
        
    }
    private void OnDestroy()
    {
        m_PlayerStats.OnLevelUp -= OnLevelUp;
    }
}
