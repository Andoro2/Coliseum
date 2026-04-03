using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Class_Fighter : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;
    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Fighter);

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
            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
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
}
