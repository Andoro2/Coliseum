using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    public int m_Cost;

    public GameObject m_RangeMesh;
    public bool m_ElementProficiency = false;

    public Dictionary<WorldElements, float> m_AttackElements = new Dictionary<WorldElements, float>();

    public TurretStatsSO m_TurretStats;

    public float m_Damage => ApplyModifiers(AffectedStat.Damage, m_TurretStats.m_Damage);
    public float m_ShootsPerMinute => ApplyModifiers(AffectedStat.Cadency, m_TurretStats.m_ShootsPerMinute);
    public float m_Range => ApplyModifiers(AffectedStat.Range, m_TurretStats.m_Range);
    public float m_ElementPercentage => ApplyModifiers(AffectedStat.ElementPercentage, m_TurretStats.m_ElementPercentage);

    void Start()
    {
        m_RangeMesh = transform.GetChild(1).gameObject;
        m_Cost = m_TurretStats.m_Price;

        ApplyLevelStats();
        IncreaseRange();

        if(m_TurretStats.Element != WorldElements.Null)
        {
            if(m_ElementProficiency) m_AttackElements.Add(m_TurretStats.Element, m_TurretStats.m_ElementPercentage + 0.5f);
            else m_AttackElements.Add(m_TurretStats.Element, m_TurretStats.m_ElementPercentage);
        }
    }

    void Update()
    {
        
    }
    public void IncreaseRange()
    {
        float effectiveRange = m_Range;
        m_RangeMesh.transform.localScale = new Vector3(
            effectiveRange,
            effectiveRange,
            m_RangeMesh.transform.localScale.z);
    }
    public void IncreaseCadency()
    {
        AddModifier(new TurretBuffSource(TurretBuffSources.Level, AffectedStat.Cadency, BuffType.Plain, WorldElements.Null, 1f, -1f));
    }
    public void IncreaseLevel()
    {
        //m_Level++;
        IncreaseRange();
    }
    public void CheckElementsOnGround()
    {
        Collider[] m_Intersecting = Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), transform.localScale / 2f);

        foreach (Collider c in m_Intersecting)
        {
            WorldElements Element = c.transform.parent.parent.GetComponent<TileElementAsigned>().TileElement;
            if (Element == m_TurretStats.Element) m_ElementProficiency = true;
        }
    }

    #region Level up

    [Header("Nivel")]
    public int m_Level = 1;
    public float m_CurrentExp;

    //public event System.Action<int> OnLevelUp;

    private float ExpToNextLevel()
    {
        return m_TurretStats.m_BaseLevelExp * Mathf.Pow(m_TurretStats.m_LevelExpGrowthFactor, m_Level - 1);
    }

    public void ObtainExp(float exp)
    {
        m_CurrentExp += exp;

        while (m_CurrentExp >= ExpToNextLevel())
        {
            m_CurrentExp -= ExpToNextLevel();
            m_Level++;

            ApplyLevelStats();
            IncreaseRange();

            //OnLevelUp?.Invoke(m_Level);
        }
    }

    private void ApplyLevelStats()
    {
        RemoveModifiersFromSource(TurretBuffSources.Level);

        int levelsAboveBase = m_Level - 1;

        float damageMultiplierBonus = Mathf.Pow(1f + m_TurretStats.m_DamageGrowthPercent, levelsAboveBase) - 1f;

        float cadencyBonus = m_TurretStats.m_CadencyGrowthFlat * levelsAboveBase;
        float rangeBonus = m_TurretStats.m_RangeGrowthFlat * levelsAboveBase;
        float elementBonus = m_TurretStats.m_ElementPercentageGrowthFlat * levelsAboveBase;

        AddModifier(new TurretBuffSource(TurretBuffSources.Level, AffectedStat.Damage, BuffType.Multiplicative, WorldElements.Null, damageMultiplierBonus, -1));
        AddModifier(new TurretBuffSource(TurretBuffSources.Level, AffectedStat.Cadency, BuffType.Plain, WorldElements.Null, cadencyBonus, -1));
        AddModifier(new TurretBuffSource(TurretBuffSources.Level, AffectedStat.Range, BuffType.Plain, WorldElements.Null, rangeBonus, -1));
        AddModifier(new TurretBuffSource(TurretBuffSources.Level, AffectedStat.ElementPercentage, BuffType.Plain, m_TurretStats.Element, elementBonus, -1));
    }

    #endregion

    #region Stats modifications
    // modifiers, buffs and debuffs
    public enum AffectedStat { Damage, Cadency, Range, ElementPercentage }
    public enum BuffType { Plain, Percentage, Multiplicative }

    public enum TurretBuffSources
    {
        Level,
        Cards,
        TurretUpgrader,
    }

    [System.Serializable]
    public class TurretBuffSource
    {
        public TurretBuffSources Source;
        public AffectedStat Stat;
        public BuffType ModType;
        public WorldElements Element;
        public float Amount;
        public float ExpirationTime;

        public bool IsExpired => ExpirationTime != -1 && Time.time >= ExpirationTime; // -1 = infinito, permanente

        public TurretBuffSource(TurretBuffSources source, AffectedStat stat, BuffType buffType, WorldElements element, float amount, float duration = -1f)
        {
            Source = source;
            Stat = stat;
            ModType = buffType;
            Element = element;
            Amount = amount;
            ExpirationTime = (duration <= 0) ? -1f : Time.time + duration;
        }
    }

    private readonly List<TurretBuffSource> m_TurretBuffs = new List<TurretBuffSource>();
    public void AddModifier(TurretBuffSource modifier) => m_TurretBuffs.Add(modifier);
    public void RemoveModifiersFromSource(TurretBuffSources source)
    {
        m_TurretBuffs.RemoveAll(m => m.Source == source);
    }
    public void RemoveExpiredModifiers()
    {
        m_TurretBuffs.RemoveAll(m => m.IsExpired);
    }
    private float ApplyModifiers(AffectedStat stat, float value)
    {
        RemoveExpiredModifiers();

        float additive = 0f;
        float percentAdd = 0f;
        float percentMult = 1f;

        foreach (var mod in m_TurretBuffs)
        {
            if (mod.Stat != stat) continue;
            switch (mod.ModType)
            {
                case BuffType.Plain: additive += mod.Amount; break;
                case BuffType.Percentage: percentAdd += mod.Amount; break;
                case BuffType.Multiplicative: percentMult *= (1f + mod.Amount); break;
            }
        }

        return (value + additive) * (1f + percentAdd) * percentMult;
    }
    #endregion


    public void IsElementProficient()
    {
        m_ElementProficiency = true;
    }
}
