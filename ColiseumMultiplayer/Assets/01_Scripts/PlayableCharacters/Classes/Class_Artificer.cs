using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Class_Artificer : MonoBehaviour
{
    private PlayerStats m_PlayerStats;

    [Header("Pasivas obtenidas:")]
    public bool m_PassiveLevel4 = false;
    public bool m_PassiveLevel8 = false;
    public bool m_PassiveLevel12 = false;
    public bool m_PassiveLevel16 = false;
    public bool m_PassiveLevel20 = false;

    [Header("Pasiva nivel 4:")]
    public bool m_ArtificerUpgrades = false;

    [Header("Pasiva nivel 12:")]
    public float m_BonusCDPercent = 0.3f;

    [Header("Pasiva nivel 20:")]
    public float m_DMGAbilityPercent = 0.5f;
    public float m_ArmorPercent = 1f;
    // Start is called before the first frame update
    void Start()
    {
        m_PlayerStats = GetComponent<PlayerStats>();

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetClassPresent(GameManager.ClassEnum.Artificer);

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
            m_ArtificerUpgrades = true;

            m_PassiveLevel4 = true;
        }
        if (newLevel >= 8 && !m_PassiveLevel8)
        {
            GameObject.FindWithTag("GameController").GetComponent<GameManager>().currencyFlag = true;
            m_PassiveLevel8 = true;
        }
        if (newLevel >= 12 && !m_PassiveLevel12)
        {
            m_PlayerStats.SetBonusCD(m_BonusCDPercent);
            m_PassiveLevel12 = true;
        }
        if (newLevel >= 16 && !m_PassiveLevel16)
        {
            m_PassiveLevel16 = true;
        }
        if (newLevel >= 20 && !m_PassiveLevel20)
        {
            m_PlayerStats.ApplyFlatArmor(m_ArmorPercent);
            m_PlayerStats.ApplyDamageBonus(m_DMGAbilityPercent);

            m_PassiveLevel20 = true;

            m_PlayerStats.OnLevelUp -= OnLevelUp;
        }
    }
}
