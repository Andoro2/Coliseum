using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class EnemyStats : NetworkBehaviour
{
    // --- Vida ---
    [Header("Vida")]
    public float m_MaxHealth;
    public NetworkVariable<float> m_CurrentHealth = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    // --- Movimiento ---
    [Header("Movimiento")]
    public float m_Speed;

    // --- Daño ---
    [Header("Daño")]
    public float m_DamageMultiplier = 1f;
    public float m_DamageToTower = 2f;

    // --- Regeneración ---
    [Header("Regeneración de vida")]
    public float m_LifeRegen = 0f;

    // --- Escudo ---
    [Header("Escudo")]
    public float m_Armor = 0f;
    private Dictionary<ShieldSource, float> m_Shields = new Dictionary<ShieldSource, float>();

    #region SHIELD MANAGEMENT
    public enum ShieldSource // escudos creados con habilidad activa primero, pasivos o regenerativos después
    {
        // Fuentess
    }
    public void SetShield(ShieldSource source, float value)
    {
        m_Shields[source] = Mathf.Max(0f, value);
    }
    public float GetTotalShield()
    {
        float total = 0f;
        foreach (float shieldValue in m_Shields.Values)
            total += shieldValue;
        return total;
    }
    private void AbsorbDamageFromShields(float damage)
    {
        foreach (ShieldSource source in System.Enum.GetValues(typeof(ShieldSource)))
        {
            if (damage <= 0f) break;
            if (m_Shields[source] <= 0f) continue; // escudo vacío, al siguiente

            if (m_Shields[source] >= damage)
            {
                m_Shields[source] -= damage;
                damage = 0f;
            }
            else
            {
                damage -= m_Shields[source];
                m_Shields[source] = 0f;
            }
        }
    }
    #endregion

    // --- Bonificadores acumulados (aplicados por pasivas) ---
    [Header("Bufos")]
    public float m_HealthBonusPercent = 0f;
    public float m_SpeedBonusPercent = 0f;
    public float m_AttackSpeedBonusPercent = 0f;
    public float m_DamageBonusPercent = 0f;
    public float m_ExpBonusPercent = 0f;
    public float m_FlatArmorBonus = 0f;
    public float m_LifeRegenBonusPercent = 0f;

    // --- Resistencias elementales ---
    // Valores entre -1 y 1. Negativo = debilidad, positivo = resistencia, 1 = inmune.
    private Dictionary<WorldElements, float> m_ElementalResistances = new Dictionary<WorldElements, float>();

    // --- Inmunidades a estados ---
    private HashSet<StatusEffect> m_Immunities = new HashSet<StatusEffect>();

    // --- UI ---
    [Header("UI")]
    private Slider m_HealthSlider;
    private TMP_Text m_HPCurrent;
    private TMP_Text m_HPMax;
    
    public event System.Action<float> OnDamageTaken;
    public event System.Action<> OnDeath;

    public enum Killer { Player, Turret }

    public event System.Action<Killer, ulong> OnDeath;

    public override void OnNetworkSpawn()
    {
        foreach (ShieldSource source in System.Enum.GetValues(typeof(ShieldSource)))
            m_Shields[source] = 0f;

        m_CurrentHealth.OnValueChanged += OnHealthChanged;

        if (IsServer)
            m_CurrentHealth.Value = m_MaxHealth;
        
        // m_HPCurrent = fightingUI.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>();
        // m_HPMax = fightingUI.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();

        // m_HealthSlider.maxValue = m_LevelsArray[m_Level].m_MaxHealth;
        // m_HealthSlider.value = m_LevelsArray[m_Level].m_MaxHealth;

        // m_HPCurrent.text = m_CurrentHealth.ToString();
        // m_HPMax.text = "/" + m_MaxHealth;
    }

    private void Update()
    {
        if(!IsServer) return;

        // m_HealthSlider.value = m_CurrentHealth;
        // m_HPCurrent.text = m_CurrentHealth.ToString();

        if (Input.GetKeyDown(KeyCode.L))
        {
            OnDamageTaken?.Invoke(TakeDamageServerRpc(5f, 0.5f, WorldElements.Null));
        }
    }

    // -------------------------------------------------------------------------
    // Recibir daño
    // -------------------------------------------------------------------------
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, float elementalPercentage, WorldElements element, KillSource source, ulong attackerClientId = 0)
    {
        float resistance = m_ResistanceMap.ContainsKey(element) ? m_ElementalResistances[element] : 0f;
        float totalDamage = (damage + damage * elementalPercentage) * (1f - resistance);

        // Escudos
        float totalShield = GetTotalShield();
        if (totalShield >= totalDamage)
        {
            AbsorbDamageFromShields(totalDamage);
            totalDamage = 0f;
        }
        else
        {
            AbsorbDamageFromShields(totalShield);
            totalDamage -= totalShield;
        }

        m_CurrentHealth.Value = Mathf.Max(0f, m_CurrentHealth.Value - totalDamage);

        if (m_CurrentHealth.Value <= 0) Die(source, attackerClientId);;
    }


    public void Die(KillSource source, ulong attackerClientId)
    {
        OnDeath?.Invoke(source, attackerClientId);
    }

    // -------------------------------------------------------------------------
    // Métodos para pasivas
    // -------------------------------------------------------------------------

    public void ApplyHealthBonus(float percent)
    {
        m_HealthBonusPercent += percent;
        m_MaxHealth = m_MaxHealth * (1f + m_HealthBonusPercent);
        m_CurrentHealth.Value = Mathf.Min(m_CurrentHealth.Value, m_MaxHealth);
        // m_HealthSlider.maxValue = m_MaxHealth;
    }

    public void ApplySpeedBonus(float percent)
    {
        m_SpeedBonusPercent += percent;
        m_Speed *= (1f + m_SpeedBonusPercent);
    }
    public void ApplyAttackSpeedBonus(float percent)
    {
        m_AttackSpeedBonusPercent += percent;
        //m_Speed = GetComponent<PlayerController>().m_Speed
        m_AttackSpeedBonusPercent *= (1f + m_SpeedBonusPercent);
    }
    public void ApplyDamageBonus(float percent)
    {
        m_DamageBonusPercent += percent;
        m_DamageMultiplier = 1f + m_DamageBonusPercent;
    }

    public void ApplyFlatArmor(float amount)
    {
        m_FlatArmorBonus += amount;
        m_Armor = m_FlatArmorBonus;
    }

    // -------------------------------------------------------------------------
    // Métodos para resistencias / inmunidades
    // -------------------------------------------------------------------------

    public void AddDamageResistancePermanent(WorldElements element, float value)
    {
        if (m_ElementalResistances.ContainsKey(element))
            m_ElementalResistances[element] = Mathf.Clamp(m_ElementalResistances[element] + value, -1f, 1f);
        else
            m_ElementalResistances[element] = Mathf.Clamp(value, -1f, 1f);
    }

    public float CheckDamageResistance(WorldElements element)
    {
        return m_ElementalResistances.ContainsKey(element) ? m_ElementalResistances[element] : 0f;
    }

    public void SetImmunity(StatusEffect effect, bool immune)
    {
        if (immune) m_Immunities.Add(effect);
        else        m_Immunities.Remove(effect);
    }

    public bool IsImmuneTo(StatusEffect effect)
    {
        return m_Immunities.Contains(effect);
    }

    // -------------------------------------------------------------------------
    // Mejoras de estadísticas
    // -------------------------------------------------------------------------

    public void SetLifeRegen()
    {

    }
    public void SetBonusCD(float CD) => m_CD += CD;

    [System.Serializable]
    public struct ElementResistance
    {
        public WorldElements element;
        [Range(-1f, 1f)] public float resistance;
    }
}