using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerStats;

public class Race_Leonin : MonoBehaviour
{
    private PlayerStats m_PlayerStats;
    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel1 = false;
    public bool m_PassiveLevel10 = false;
    public bool m_PassiveLevel20 = false;

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
        if (newLevel >= 1 && !m_PassiveLevel1)
        {
            m_PassiveLevel1 = true;
        }
        if (newLevel >= 10 && !m_PassiveLevel10)
        {
            m_PassiveLevel10 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
    private void OnDestroy()
    {
        m_PlayerStats.OnLevelUp -= OnLevelUp;
    }
}
