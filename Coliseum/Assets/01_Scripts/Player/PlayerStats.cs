using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    // --- Vida ---
    [Header("Vida")]
    public float m_MaxHealth;
    public float m_CurrentHealth;

    // --- Movimiento ---
    [Header("Movimiento")]
    public float m_Speed;

    // --- Daño ---
    [Header("Daño")]
    public float m_DamageMultiplier = 1f;

    // --- Armadura ---
    [Header("Armadura")]
    public float m_Armor = 0f;

    // --- Regeneración ---
    [Header("Regeneración de vida")]
    public float m_LifeRegen = 0f;

    // --- Enfriamiento de habilidades ---
    [Header("Enfriamiento de habilidades")]
    public float m_CD = 0f;

    // --- Escudo ---
    [Header("Escudo")]
    private Dictionary<ShieldSource, float> m_Shields = new Dictionary<ShieldSource, float>();
    private Dictionary<DynamicDamageSource, float> m_DynamicDamageBonus = new Dictionary<DynamicDamageSource, float>();
    private Dictionary<DynamicLifeRegenSource, float> m_DynamicLifeRegenBonus = new Dictionary<DynamicLifeRegenSource, float>();

    public enum DynamicDamageSource
    {
        BarbarianWrath,
    }
    public enum DynamicLifeRegenSource
    {
        BarbarianWrath,
    }

    public void SetDynamicDamageBonus(DynamicDamageSource source, float value)
    {
        m_DynamicDamageBonus[source] = value;
        RecalculateDamage();
    }
    private void RecalculateDamage()
    {
        float total = 1f + m_DamageBonusPercent;
        foreach (float bonus in m_DynamicDamageBonus.Values)
            total += bonus;
        m_DamageMultiplier = total;
    }

    public void SetDynamicLifeRegenBonus(DynamicLifeRegenSource source, float value)
    {
        m_DynamicLifeRegenBonus[source] = value;
        RecalculateLifeRegen();
    }
    private void RecalculateLifeRegen()
    {
        float total = m_LifeRegenBonusPercent;
        foreach (float bonus in m_DynamicLifeRegenBonus.Values)
            total += bonus;
        m_LifeRegen = total;
    }

    #region SHIELD MANAGEMENT
    public enum ShieldSource // escudos creados con habilidad activa primero, pasivos o regenerativos después
    {
        GoliathShield,
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

    // --- Experiencia y nivel ---
    [Header("Nivel")]
    public int m_Level = 0;
    public float m_CurrentExp;
    public List<LevelAttributes> m_LevelsArray;

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
    private Slider m_ExpSlider;
    private TMP_Text m_HPCurrent;
    private TMP_Text m_HPMax;

    // --- Recurso según la clase ---
    [Header("Recursos de clase")]
    public float m_Scrap;
    public float m_MaxWrath;
    public float m_Wrath;
    public float m_Ki;
    public float m_Magic;
    private float m_WrathStatBonus;

    // --- Getters y eventos ---
    public float GetWrathStatBonus() => m_WrathStatBonus;

    public event System.Action<int> OnLevelUp;
    public event System.Action<float> OnDamageTaken;

    private PlayerController PC;

    private void Start()
    {
        PC = transform.parent.GetComponent<PlayerController>();
        PC.AbilityQUsed += AbilityQ;
        PC.AbilityEUsed += AbilityE;
        PC.UltimateUsed += Ultimate;

        foreach (ShieldSource source in System.Enum.GetValues(typeof(ShieldSource)))
            m_Shields[source] = 0f;
        foreach (DynamicDamageSource source in System.Enum.GetValues(typeof(DynamicDamageSource)))
            m_DynamicDamageBonus[source] = 0f;
        foreach (DynamicLifeRegenSource source in System.Enum.GetValues(typeof(DynamicLifeRegenSource)))
            m_DynamicLifeRegenBonus[source] = 0f;

        m_CurrentHealth = m_MaxHealth;

        // UI — misma lógica que tenías en PlayerController
        GameObject fightingUI = GameObject.FindWithTag("UICanvas").transform.GetChild(0).GetChild(0).gameObject;

        m_ExpSlider = fightingUI.transform.GetChild(0).GetComponent<Slider>();
        m_HealthSlider = fightingUI.transform.GetChild(1).GetComponent<Slider>();
        m_HPCurrent = fightingUI.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>();
        m_HPMax = fightingUI.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();

        m_HealthSlider.maxValue = m_LevelsArray[m_Level].m_MaxHealth;
        m_HealthSlider.value = m_LevelsArray[m_Level].m_MaxHealth;

        m_ExpSlider.minValue = 0;
        m_ExpSlider.maxValue = m_LevelsArray[m_Level].m_ExpToAdvance;

        m_HPCurrent.text = m_CurrentHealth.ToString();
        m_HPMax.text = "/" + m_MaxHealth;
    }

    private void Update()
    {
        m_HealthSlider.value = m_CurrentHealth;
        m_ExpSlider.value = m_CurrentExp;
        m_HPCurrent.text = m_CurrentHealth.ToString();

        if (m_CurrentHealth <= 0)
            Die();

        if (Input.GetKeyDown(KeyCode.O))
        {
            OnDamageTaken?.Invoke(m_Level);
        }
    }

    public void AbilityQ()
    {
        Debug.Log("Habilidad Q");
    }
    public void AbilityE()
    {
        Debug.Log("Habilidad E");
    }
    public void Ultimate()
    {
        Debug.Log("ULTIMATE");
    }

    // -------------------------------------------------------------------------
    // Daño y curación
    // -------------------------------------------------------------------------

    public void TakeDamage(float damage, WorldElements element = WorldElements.Null)
    {
        // Notifica a los scripts suscritos que se ha recibido daño
        OnDamageTaken?.Invoke(damage);

        float resistance = m_ElementalResistances.ContainsKey(element)
            ? m_ElementalResistances[element] : 0f;

        float totalShield = GetTotalShield();

        if (totalShield >= damage)
        {
            // El escudo absorbe todo el daño
            AbsorbDamageFromShields(damage);
            damage = 0f;
        }
        else
        {
            // El escudo absorbe lo que puede y el resto va a la vida
            AbsorbDamageFromShields(totalShield);
            damage -= totalShield;
        }

        float finalDamage = damage * (1f - resistance);
        finalDamage = Mathf.Max(0f, finalDamage - m_Armor);

        m_CurrentHealth -= finalDamage;
    }

    public void Heal(float amount)
    {
        m_CurrentHealth = Mathf.Min(m_CurrentHealth + amount, m_MaxHealth);
    }

    private void Die()
    {
        // Tu lógica de muerte aquí
    }

    // -------------------------------------------------------------------------
    // Experiencia y nivel
    // -------------------------------------------------------------------------

    public void ObtainExp(float exp)
    {
        // Aplica el bonus de experiencia de pasivas (ej: Humano +10%, Alto Elfo +50% radio)
        m_CurrentExp += exp * (1f + m_ExpBonusPercent);

        if (m_CurrentExp >= m_LevelsArray[m_Level].m_ExpToAdvance
            && m_Level + 1 < m_LevelsArray.Count)
        {
            m_Level++;

            m_MaxHealth      = m_LevelsArray[m_Level].m_MaxHealth;
            m_CurrentHealth  = m_MaxHealth;

            m_HealthSlider.maxValue = m_MaxHealth;
            m_HealthSlider.value    = m_MaxHealth;

            m_ExpSlider.minValue = m_LevelsArray[m_Level - 1].m_ExpToAdvance;
            m_ExpSlider.maxValue = m_LevelsArray[m_Level].m_ExpToAdvance;

            m_HPMax.text = "/" + m_MaxHealth;

            OnLevelUp?.Invoke(m_Level);
        }
    }

    // -------------------------------------------------------------------------
    // Métodos para pasivas
    // -------------------------------------------------------------------------

    public void ApplyHealthBonus(float percent)
    {
        m_HealthBonusPercent += percent;
        m_MaxHealth = m_LevelsArray[m_Level].m_MaxHealth * (1f + m_HealthBonusPercent);
        m_CurrentHealth = Mathf.Min(m_CurrentHealth, m_MaxHealth);
        m_HealthSlider.maxValue = m_MaxHealth;
    }
    public void ApplySpeedBonus(float percent)
    {
        m_SpeedBonusPercent += percent;
        m_Speed = transform.parent.GetComponent<PlayerController>().m_Speed * (1f + m_SpeedBonusPercent);
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

    public void ApplyExpBonus(float percent)
    {
        m_ExpBonusPercent += percent;
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

    /*public void IncreaseStatOnLife(CharStats stat, float increase, )
    {

    }*/


    // -------------------------------------------------------------------------
    // Mejoras de estadísticas
    // -------------------------------------------------------------------------

    public void SetLifeRegen()
    {

    }
    public void SetBonusCD(float CD) => m_CD += CD;

    // -------------------------------------------------------------------------
    // Ajustes de estadísticas por clase
    // -------------------------------------------------------------------------

    // BARBARIAN
    public void StackWrath(float wrath)
    {
        if ((m_Wrath + wrath) > m_MaxWrath) m_Wrath = m_MaxWrath;
        else m_Wrath += wrath;

        m_WrathStatBonus = m_Wrath * 0.01f;
    }

    // -------------------------------------------------------------------------
    // Clases de datos
    // -------------------------------------------------------------------------

    [System.Serializable]
    public class LevelAttributes
    {
        public int m_Level;
        public float m_MaxHealth;
        public float m_ExpToAdvance;
    }
}