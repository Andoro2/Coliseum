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

    public int m_Damage,
        m_ShootsPerMinute,
        m_Range,
        m_Price;
}
