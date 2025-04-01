using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class EnemyMovement : MonoBehaviour
{
    public float m_Health, m_Speed = 5f;
    public List<Transform> m_Path = new List<Transform>();
    //public List<GameObject> m_PathSelectorFatherList = new List<GameObject>();

    void Start()
    {
        
    }

    void Update()
    {
        if (m_Path.Count > 0)
        {
            Move();
        }
    }
    public void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, m_Path[0].position, m_Speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, m_Path[0].position) <= 0.25f)
        {
            m_Path.RemoveAt(0);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PathSelector"))
        {
            AssignPath(other.gameObject);
            /*if(!m_PathSelectorFatherList.Contains(other.gameObject.transform.parent.gameObject))
            {
                AssignPath(other.gameObject.transform.parent.gameObject);
            }*/
        }
    }

    public void AssignPath(GameObject PathSelector)
    {
        m_Path.Clear();

        GameObject m_PathSelectorFather = PathSelector.gameObject.transform.parent.gameObject;

        //m_PathSelectorFatherList.Add(m_PathSelectorFather);

        GameObject PathContainer = m_PathSelectorFather.transform.GetChild(1).gameObject;

        for (int i = 0; i < PathContainer.transform.childCount; i++)
        {
            Transform child = PathContainer.transform.GetChild(i);

            m_Path.Add(child.gameObject.transform);
        }

        HashSet<Transform> uniqueChildren = new HashSet<Transform>(m_Path);

        m_Path = new List<Transform>(uniqueChildren);
    }
    /*private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Turret"))
        {
            other.GetComponent<Ballista>().EnemyLeavesRange(gameObject);
        }
    }*/
}
