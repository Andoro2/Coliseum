using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    public int m_Level = 1;
    private int leveles;
    public GameObject m_RangeMesh;
    // Start is called before the first frame update
    void Start()
    {
        m_RangeMesh = transform.GetChild(1).gameObject;
        leveles = m_Level;
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
        1000f + 250f * m_Level,
        1000f + 250f * m_Level,
        m_RangeMesh.transform.localScale.z
    );
    }
    public void IncreaseLevel()
    {
        //m_Level++;
        IncreaseRange();
    }
}
