using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

[CreateAssetMenu(fileName = "TurretStats", menuName = "ScriptableObjects/TowerStats")]
public class TurretStatsSO: ScriptableObject
{
    public string m_TurretName;

    public WorldElements Element = WorldElements.Null;
    public float m_ElementPercentage = 0;

    public float m_Damage,
        m_ShootsPerMinute,
        m_Range;
    public int m_Price;

    [Header("Mejora por nivel:")]
    public float m_DamageGrowthPercent = 0.1f,
        m_CadencyGrowthFlat = 0.5f,
        m_RangeGrowthFlat = 20f,
        m_ElementPercentageGrowthFlat = 0f;

    [Header("Curva de exp:")]
    public float m_BaseLevelExp = 100f,
        m_LevelExpGrowthFactor = 1.15f;
}
