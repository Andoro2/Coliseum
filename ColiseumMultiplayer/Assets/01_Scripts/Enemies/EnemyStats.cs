using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static EnemyStats;
using static UnityEngine.GraphicsBuffer;

public class EnemyStats : NetworkBehaviour
{
    public enum EnemyClasses
    {
        Runner,
        Fighter,
        Elite,
        RoundBoss,
        FinalBoss
    }
    public EnemyClasses m_EnemyClass;

    public enum EnemyTypes
    {
        Aberration,
        Beast,
        Celestial,
        Construct,
        Dragon,
        Elemental,
        Fey,
        Fiend,
        Giant,
        Humanoid,
        Monstrosity,
        Ooze,
        Plant,
        Undead
    }
    public List<EnemyTypes> m_EnemyTypeList = new List<EnemyTypes>();

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

    #region SHIELD MANAGEMENT
    public enum ShieldPriority { Passive = 0, Permanent = 1 } // para ordenar primero los pasivos y luego los permanentes

    private List<ShieldInstance> m_ShieldList = new List<ShieldInstance>();

    [System.Serializable]
    public class ShieldInstance
    {
        public WorldElements Element;
        public float Amount;
        public ShieldPriority Priority;
        public float ExpirationTime;

        public bool IsExpired => ExpirationTime != -1 && Time.time >= ExpirationTime; // -1 = infinito, permanente

        public ShieldInstance(WorldElements element, float amount, ShieldPriority priority, float duration = -1f)
        {
            Element = element;
            Amount = amount;
            Priority = priority;
            ExpirationTime = (duration <= 0) ? -1f : Time.time + duration;
        }
    }

    public void AddShield(WorldElements element, float amount, ShieldPriority priority, float duration = -1f)
    {
        m_ShieldList.Add(new ShieldInstance(element, amount, priority, duration));

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
    
    public enum Killer { Player, Turret }

    public event System.Action<Killer, ulong> OnDeath;
    public event System.Action<float, WorldElements> OnDamageTaken;
    //public event System.Action<float> OnHealthChanged;
    public static event System.Action<Vector3, EnemyStats.Killer, ulong> OnAnyEnemyDeath;


    public GameObject DamageText;
    public override void OnNetworkSpawn()
    {
        //m_CurrentHealth.OnValueChanged += OnHealthChanged;

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
            //TakeDamageServerRpc(5f, 0.5f, WorldElements.Null, Killer.Player, NetworkManager.Singleton.LocalClientId);
        }
    }

    // -------------------------------------------------------------------------
    // Recibir daño
    // -------------------------------------------------------------------------
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ElementDamage[] attackElements, bool isCrit, float critExtra, Killer source, ulong attackerClientId = 0)
    {
        if (attackElements == null || attackElements.Length == 0)
            attackElements = new ElementDamage[] { new ElementDamage { Element = WorldElements.Null, Percentage = 1f } };

        //float totalLifeDamage = 0f;

        foreach (ElementDamage ed in attackElements)
        {
            if (m_CurrentHealth.Value <= 0) break;

            float resistance = CheckDamageResistance(ed.Element);
            float initialDmg;
            //float elementDmg;
            float remainingDmg;

            if (isCrit) initialDmg = (damage * critExtra);
            else initialDmg = damage;
            //if (isCrit) initialDmg = (damage * critExtra * ed.Percentage) * (1f - resistance);
            //else initialDmg = (damage * ed.Percentage) * (1f - resistance);

            //elementDmg = AbsorbElementalDamage(ed.Element, initialDmg * ed.Percentage);

            if (ed.Element == WorldElements.Null) remainingDmg = initialDmg;
            else remainingDmg = AbsorbElementalDamage(ed.Element, initialDmg * ed.Percentage);

            //remainingDmg = Mathf.Max(0f, remainingDmg - m_Armor);

            if (remainingDmg > 0)
            {
                m_CurrentHealth.Value = Mathf.Max(0f, m_CurrentHealth.Value - remainingDmg);

                OnDamageTaken?.Invoke(remainingDmg, ed.Element);

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

        // cleric level 12
        if (NetworkManager.Singleton.LocalClientId == attackerClientId)
        {
            Class_Cleric isCleric = PlayerController.LocalInstance.GetComponentInChildren<Class_Cleric>();
            if (isCleric != null) if(m_EnemyTypeList.Contains(EnemyTypes.Undead) && isCleric.m_PassiveLevel12) Die(source, attackerClientId);
        }

        if (m_CurrentHealth.Value <= 0) Die(source, attackerClientId);
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
        var textElement = damageText.GetComponent<DamageTextElement>();
        if (textElement != null)
        {
            textElement.GetDamageInfo(element, damageAmount);

            // if (isCrit) textElement.SetCriticalStyle(); 
        }
        */
    }

    private bool m_IsDead = false;
    public void Die(Killer source, ulong attackerClientId)
    {
        //pot ser a vegades s'invoque molt ràpid i de duplique d'alguna forma, es per a evitar-ho
        if (m_IsDead) return;
        m_IsDead = true;

        OnDeath?.Invoke(source, attackerClientId);
        NotifyAnyDeathClientRpc(transform.position, source, attackerClientId);

        if (m_EnemyClass == EnemyClasses.Elite || m_EnemyClass == EnemyClasses.RoundBoss)
            NotifyDeathClientRpc(source, attackerClientId, m_EnemyClass);

        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void NotifyAnyDeathClientRpc(Vector3 position, Killer source, ulong attackerClientId)
    {
        OnAnyEnemyDeath?.Invoke(position, source, attackerClientId);
    }

    // detectar info del eliminador
    [ClientRpc]
    private void NotifyDeathClientRpc(Killer source, ulong attackerClientId, EnemyClasses enemyClass)
    {
        if (source != Killer.Player) return;
        if (NetworkManager.Singleton.LocalClientId != attackerClientId) return;

        Class_Bard isBard = PlayerController.LocalInstance.GetComponentInChildren<Class_Bard>();
        if (isBard != null) isBard.LegendBuff(true, enemyClass);
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
    //public void SetBonusCD(float CD) => m_CD += CD;

    [System.Serializable]
    public struct ElementResistance
    {
        public WorldElements element;
        [Range(-1f, 1f)] public float resistance;
    }
}