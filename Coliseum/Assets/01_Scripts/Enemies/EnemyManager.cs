using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public struct ElementResistance
    {
        public WorldElements element;
        [Range(-1f, 1f)] public float resistance; // -1 = muy débil, 0 = neutro, 1 = inmune
    }

    public float m_Health = 5f;
    public Slider m_HealthSlider;
    public int m_Value = 50, m_ExpValue = 10;

    public List<ElementResistance> m_Resistances = new List<ElementResistance>();

    // Diccionario interno generado al inicio, para consultas rápidas en runtime
    private Dictionary<WorldElements, float> m_ResistanceMap = new Dictionary<WorldElements, float>();

    public List<State> m_States = new List<State>();

    void Start()
    {
        m_HealthSlider.maxValue = m_Health;
        m_HealthSlider.value = m_Health;

        // Construimos el diccionario a partir de la lista del Inspector
        m_ResistanceMap.Clear();
        foreach (ElementResistance r in m_Resistances)
            m_ResistanceMap[r.element] = r.resistance;
    }

    void Update()
    {
        m_HealthSlider.value = m_Health;

        if (m_Health <= 0)
            Death();
    }

    public class State
    {
        EnemySpawner.States ActiveState;
        float m_Intensity, m_ActiveTime;
    }

    public void Death()
    {
        foreach (GameObject turret in GetComponent<EnemyMovement>().m_TurretsTargetedBy)
        {
            if (turret.GetComponent<InRangeManager>().enemiesInRange.Contains(gameObject))
                turret.GetComponent<InRangeManager>().RemoveFromList(gameObject);
        }

        GameObject.FindWithTag("GameController").GetComponent<GameManager>().GetPaid(m_Value);
        GameObject.FindWithTag("Player").GetComponent<PlayerStats>().ObtainExp(m_ExpValue);

        Destroy(gameObject);
    }

    public void TakeDamage(float damage, float elementalPercentage, WorldElements element)
    {
        float resistance = m_ResistanceMap.ContainsKey(element) ? m_ResistanceMap[element] : 0f;
        float totalDamage = (damage + damage * elementalPercentage) * (1f - resistance);
        m_Health -= totalDamage;
    }

    public bool IsWeakTo(WorldElements element)
    {
        return m_ResistanceMap.ContainsKey(element) && m_ResistanceMap[element] < 0f;
    }
}