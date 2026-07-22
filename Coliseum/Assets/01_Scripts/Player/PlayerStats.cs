using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static PlayerStats;

public class PlayerStats : NetworkBehaviour
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
    public float m_Damage = 15f;
    public float m_DamageMultiplier = 1f;
    public float m_CriticChance = 0.1f;
    public float m_CriticExtra = 1.5f;

    public List<GameObject> m_AreaAttackPrefabs = new List<GameObject>();

    // --- Armadura ---
    [Header("Armadura")]
    public float m_Armor = 0f;
    public float m_CriticResistance = 0f;

    // --- Regeneración ---
    [Header("Regeneración de vida")]
    public float m_LifeRegen = 0f;

    // --- Enfriamiento de habilidades ---
    [Header("Enfriamiento de habilidades")]
    public float m_CD = 0f;

    // --- Damage ---
    private Dictionary<DynamicDamageSource, float> m_DynamicDamageBonus = new Dictionary<DynamicDamageSource, float>();
    public enum DynamicDamageSource
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
    
    public Dictionary<WorldElements, float> m_AutoAttackElements = new Dictionary<WorldElements, float>();

    public void AddAutoAttackElement(WorldElements element, float percentage)
    {
        if (m_AutoAttackElements.ContainsKey(element))
            m_AutoAttackElements[element] = Mathf.Clamp01(m_AutoAttackElements[element] + percentage);
        else
            m_AutoAttackElements[element] = Mathf.Clamp01(percentage);
    }

    public void RemoveAutoAttackElement(WorldElements element, float percentage)
    {
        if (!m_AutoAttackElements.ContainsKey(element)) return;

        m_AutoAttackElements[element] -= percentage;

        if(m_AutoAttackElements[element] <= 0)
            m_AutoAttackElements.Remove(element);
    }

    // --- Regeneración de vida ---
    private Dictionary<DynamicLifeRegenSource, float> m_DynamicLifeRegenBonus = new Dictionary<DynamicLifeRegenSource, float>();
    public enum DynamicLifeRegenSource
    {
        BarbarianWrath,
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

    // --- Escudo ---

    #region SHIELD MANAGEMENT
    public enum ShieldPriority { Passive = 0, Permanent = 1 } // para ordenar primero los pasivos y luego los permanentes

    public enum ShieldSources
    {
        GoliathShield,
    }

    private List<ShieldInstance> m_ShieldList = new List<ShieldInstance>();

    [System.Serializable]
    public class ShieldInstance
    {
        public ShieldSources Source;
        public WorldElements Element;
        public float Amount;
        public ShieldPriority Priority;
        public float ExpirationTime;

        public bool IsExpired => ExpirationTime != -1 && Time.time >= ExpirationTime; // -1 = infinito, permanente

        public ShieldInstance(ShieldSources source, WorldElements element, float amount, ShieldPriority priority, float duration = -1f)
        {
            Source = source;
            Element = element;
            Amount = amount;
            Priority = priority;
            ExpirationTime = (duration <= 0) ? -1f : Time.time + duration;
        }
    }

    public void AddShield(ShieldSources sourde, WorldElements element, float amount, ShieldPriority priority, float duration = -1f)
    {
        m_ShieldList.Add(new ShieldInstance(sourde, element, amount, priority, duration));

        m_ShieldList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public float GetTotalShield()
    {
        float total = 0f;

        m_ShieldList.RemoveAll(s => s.IsExpired || s.Amount <= 0);

        foreach (var shield in m_ShieldList)
            total += shield.Amount;
        return total;
    }

    private float AbsorbElementalDamage(WorldElements element, float damage)
    {
        m_ShieldList.RemoveAll(s => s.IsExpired || s.Amount <= 0);

        for (int i = 0; i < m_ShieldList.Count; i++)
        {
            if (damage <= 0) break;

            if (m_ShieldList[i].Element == element)
            {
                if (m_ShieldList[i].Amount >= damage)
                {
                    m_ShieldList[i].Amount -= damage;
                    damage = 0;
                }
                else
                {
                    damage -= m_ShieldList[i].Amount;
                    m_ShieldList[i].Amount = 0;
                }
            }
        }

        m_ShieldList.RemoveAll(s => s.Amount <= 0);
        return damage;
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
    public GameObject DamageText;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            m_CurrentHealth.Value = m_MaxHealth;
        }

        m_CurrentHealth.OnValueChanged += (oldValue, newValue) => {
            if (m_HealthSlider != null) m_HealthSlider.value = newValue;
            if (m_HPCurrent != null) m_HPCurrent.text = Mathf.CeilToInt(newValue).ToString();
        };

        // Aquí pones el resto de tu lógica de inicialización de UI que tenías en Start
    }
    private void Awake()
    {
        m_Level = 1;
        if (m_LevelsArray != null && m_LevelsArray.Count > 1)
            m_MaxHealth = m_LevelsArray[m_Level].m_MaxHealth;
    }
    private void Start()
    {
        PC = transform.parent.GetComponent<PlayerController>();
        PC.AbilityQUsed += AbilityQ;
        PC.AbilityEUsed += AbilityE;
        PC.UltimateUsed += Ultimate;

        m_Level = 1;
        m_MaxHealth = m_LevelsArray[m_Level].m_MaxHealth;

        foreach (DynamicDamageSource source in System.Enum.GetValues(typeof(DynamicDamageSource)))
            m_DynamicDamageBonus[source] = 0f;
        foreach (DynamicLifeRegenSource source in System.Enum.GetValues(typeof(DynamicLifeRegenSource)))
            m_DynamicLifeRegenBonus[source] = 0f;
        foreach (SpawnableEntry entry in m_SpawnableList)
            m_SpawnablePrefabs[entry.Key] = entry.Prefab;

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
        m_HealthSlider.value = m_CurrentHealth.Value;
        m_ExpSlider.value = m_CurrentExp;
        m_HPCurrent.text = m_CurrentHealth.ToString();

        if (m_CurrentHealth.Value <= 0)
            Die();

        if (Input.GetKeyDown(KeyCode.O))
        {
            OnDamageTaken?.Invoke(10f);
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

    public void TakeDamage(float damage, ElementDamage[] attackElements, bool isCrit, float critExtra)
    {
        if (attackElements == null || attackElements.Length == 0)
            attackElements = new ElementDamage[] { new ElementDamage { Element = WorldElements.Null, Percentage = 1f } };

        foreach (ElementDamage ed in attackElements)
        {
            float resistance = CheckDamageResistance(ed.Element);
            float initialDmg;
            //float elementDmg;
            float remainingDmg;

            if (isCrit) initialDmg = (damage * critExtra);
            else initialDmg = damage;
            //if (isCrit) initialDmg = (damage * critExtra * ed.Percentage) * (1f - resistance);
            //else initialDmg = (damage * ed.Percentage) * (1f - resistance);

            //float remainingDmg = AbsorbElementalDamage(ed.Element, initialDmg);
            if (ed.Element == WorldElements.Null) remainingDmg = initialDmg;
            else remainingDmg = AbsorbElementalDamage(ed.Element, initialDmg * ed.Percentage);

            if (remainingDmg > 0)
            {
                m_CurrentHealth.Value = Mathf.Max(0f, m_CurrentHealth.Value - remainingDmg);

                //totalLifeDamage += remainingDmg;

                ShowDamageTextClientRpc(remainingDmg, ed, isCrit);
                /*
                GameObject damageText = Instantiate(DamageText, transform.position, transform.rotation);

                damageText.GetComponent<DamageTextElement>().GetDamageInfo(
                    ed.Element,
                    remainingDmg
                );
                */
            }
        }

        //totalLifeDamage = Mathf.Max(0f, totalLifeDamage - m_Armor); // REVISAR TEMA DE LA ARMADURA


        if (m_CurrentHealth.Value <= 0) Die();
    }
    [ClientRpc]
    private void ShowDamageTextClientRpc(float damageAmount, ElementDamage element, bool isCrit)
    {
        Vector3 spawnOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0.5f, 1.5f),
            Random.Range(-0.5f, 0.5f)
        );

        GameObject damageText = Instantiate(DamageText, transform.position + spawnOffset, Quaternion.identity);

        damageText.GetComponentInChildren<DamageTextElement>().GetDamageInfo(element, damageAmount);
        /*
        var textElement = damageText.GetComponentInChildren<DamageTextElement>();
        if (textElement != null)
        {
            textElement.GetDamageInfo(element, damageAmount);

            // if (isCrit) textElement.SetCriticalStyle(); 
        }
        */
    }

    public void Heal(float amount)
    {
        m_CurrentHealth.Value = Mathf.Min(m_CurrentHealth.Value + amount, m_MaxHealth);
    }

    private void Die()
    {
        // -------------------------------------------------------------------------
        // Revive level bard 20
        // -------------------------------------------------------------------------
        if (GetComponent<Class_Cleric>() != null)
        {
            if (GetComponent<Class_Cleric>().m_ClericRevive)
            {
                GetComponentInParent<PlayerController>().HealServerRpc(m_MaxHealth/2);
                GetComponent<Class_Cleric>().ClericReliveSwitch();
            }
        }
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
            m_CurrentHealth.Value  = m_MaxHealth;

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
        m_CurrentHealth.Value = Mathf.Min(m_CurrentHealth.Value, m_MaxHealth);
        m_HealthSlider.maxValue = m_MaxHealth;
    }
    public void ApplyDamageBonus(float percent)
    {
        m_DamageBonusPercent += percent;
        m_DamageMultiplier = 1f + m_DamageBonusPercent;
    }
    public void ApplyAttackSpeedBonus(float percent)
    {
        m_AttackSpeedBonusPercent += percent;
        //m_Speed = GetComponent<PlayerController>().m_Speed
        m_AttackSpeedBonusPercent *= (1f + m_SpeedBonusPercent);
    }
    public void ApplySpeedBonus(float percent)
    {
        m_SpeedBonusPercent += percent;
        m_Speed = transform.parent.GetComponent<PlayerController>().m_Speed * (1f + m_SpeedBonusPercent);
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

    public void GetCritChance(float extraChance)
    {
        m_CriticChance += extraChance;
    }
    public void GetCritDamage(float extraDmg)
    {
        m_CriticExtra += extraDmg;
    }
    public void SetLifeRegen()
    {

    }
    public void SetBonusCD(float CD) => m_CD += CD;

    // drops on enemykill
    public enum SpawnableObject
    {
        ClericAreaL4,
        ClericHealL8,
    }
    public Dictionary<SpawnableObject, GameObject> m_SpawnablePrefabs = new Dictionary<SpawnableObject, GameObject>();

    [System.Serializable]
    public class SpawnableEntry
    {
        public SpawnableObject Key;
        public GameObject Prefab;
    }

    public List<SpawnableEntry> m_SpawnableList = new List<SpawnableEntry>();

    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectServerRpc(Vector3 position, SpawnableObject objectType)
    {
        if (!m_SpawnablePrefabs.ContainsKey(objectType)) return;
        GameObject item = Instantiate(m_SpawnablePrefabs[objectType], position, Quaternion.identity);
        item.GetComponent<NetworkObject>().Spawn();
    }

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