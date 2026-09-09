using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    public int m_Cost;
    public float m_Cadency;

    public GameObject m_RangeMesh;
    public bool m_ElementProficiency = false;

    public TurretStatsSO m_TurretStats;

    public float m_Damage => ApplyModifiers(AffectedStat.Damage, m_TurretStats.m_Damage * m_Level);
    public float m_ShootsPerMinute => ApplyModifiers(AffectedStat.Cadency, m_TurretStats.m_ShootsPerMinute);
    public float m_Range => ApplyModifiers(AffectedStat.Range, m_TurretStats.m_Range + 250f * m_Level);
    public float m_ElementPercentage => ApplyModifiers(AffectedStat.ElementPercentage, m_TurretStats.m_ElementPercentage);

    // Start is called before the first frame update
    void Start()
    {
        m_RangeMesh = transform.GetChild(1).gameObject;
        //leveles = m_Level;
        m_Cost = m_TurretStats.m_Price;

        ApplyLevelStats();
        IncreaseRange();
        //m_Cadency = m_TurretStats.m_ShootsPerMinute;
        //m_ElementPercentage = m_TurretStats.m_ElementPercentage;
    }

    // Update is called once per frame
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
        AddModifier(new StatModifier(AffectedStat.Cadency, Modification.Plain, 1f, this));
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

    public event System.Action<int> OnLevelUp;

    private static readonly object LevelGrowthSource = new object();

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

            OnLevelUp?.Invoke(m_Level);
        }
    }

    private void ApplyLevelStats()
    {
        RemoveModifiersFromSource(LevelGrowthSource);

        int levelsAboveBase = m_Level - 1;

        float damageMultiplierBonus = Mathf.Pow(1f + m_TurretStats.m_DamageGrowthPercent, levelsAboveBase) - 1f;

        float cadencyBonus = m_TurretStats.m_CadencyGrowthFlat * levelsAboveBase;
        float rangeBonus = m_TurretStats.m_RangeGrowthFlat * levelsAboveBase;
        float elementBonus = m_TurretStats.m_ElementPercentageGrowthFlat * levelsAboveBase;

        AddModifier(new StatModifier(AffectedStat.Damage, Modification.Multiplicative, damageMultiplierBonus, LevelGrowthSource));
        AddModifier(new StatModifier(AffectedStat.Cadency, Modification.Plain, cadencyBonus, LevelGrowthSource));
        AddModifier(new StatModifier(AffectedStat.Range, Modification.Plain, rangeBonus, LevelGrowthSource));
        AddModifier(new StatModifier(AffectedStat.ElementPercentage, Modification.Plain, elementBonus, LevelGrowthSource));
    }

    #endregion

    #region Stats modifications
    // modifiers, buffs and debuffs
    public enum AffectedStat { Damage, Cadency, Range, ElementPercentage }
    public enum Modification { Plain, Percentage, Multiplicative }

    public struct StatModifier
    {
        public AffectedStat Stat;
        public Modification Effect;
        public float Value;
        public object Source;

        public StatModifier(AffectedStat stat, Modification mod, float value, object source)
        {
            Stat = stat;
            Effect = mod;
            Value = value;
            Source = source;
        }
    }

    private readonly List<StatModifier> m_Modifiers = new List<StatModifier>();
    public void AddModifier(StatModifier modifier) => m_Modifiers.Add(modifier);
    public void RemoveModifiersFromSource(object source) => m_Modifiers.RemoveAll(m => m.Source == source);

    private float ApplyModifiers(AffectedStat stat, float baseValue)
    {
        float additive = 0f;
        float percentAdd = 0f;
        float percentMult = 1f;

        foreach (var mod in m_Modifiers)
        {
            if (mod.Stat != stat) continue;
            switch (mod.Effect)
            {
                case Modification.Plain: additive += mod.Value; break;
                case Modification.Percentage: percentAdd += mod.Value; break;
                case Modification.Multiplicative: percentMult *= (1f + mod.Value); break;
            }
        }

        return (baseValue + additive) * (1f + percentAdd) * percentMult;
    }
    #endregion
}
