using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;
using static TurretStatsSO;

public class TowerStats : MonoBehaviour
{
    
    public int m_Level = 1;
    private int leveles;
    static float m_Cost,
        m_Cadency;
    public GameObject m_RangeMesh;
    public bool m_ElementProficiency = false;

    public TurretStatsSO m_TurretStats;
    // Start is called before the first frame update
    void Start()
    {
        m_RangeMesh = transform.GetChild(1).gameObject;
        leveles = m_Level;
        m_Cost = m_TurretStats.m_Price;
        m_Cadency = m_TurretStats.m_ShootsPerMinute;
    }

    // Update is called once per frame
    void Update()
    {
        if(leveles != m_Level)
        {
            IncreaseLevel();
            leveles = m_Level;
        }
    }
    public void IncreaseRange()
    {
        m_RangeMesh.transform.localScale = new Vector3(
        m_TurretStats.m_Range + 250f * m_Level,
        m_TurretStats.m_Range + 250f * m_Level,
        m_RangeMesh.transform.localScale.z);
    }
    public void IncreaseCadency()
    {
        m_TurretStats.m_ShootsPerMinute++;
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
}
