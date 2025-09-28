using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "ScriptableObjects/EnemyStats")]

public class EnemyStatsSO : ScriptableObject
{
    public string m_EnemyName;

    public List<ElementResistance> m_Resistancies = new List<ElementResistance>();

    public int m_Health,
        m_Reward,
        m_Speed,
        m_Experience,
        m_TowerDamage;

    [SerializeField]
    public class ElementResistance
    {
        public WorldElements Element;
        public float Resistance;
    }
}
