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
        }
        if (other.CompareTag("Finish"))
        {
            Destroy(gameObject);
        }
    }

    public void AssignPath(GameObject PathSelector)
    {
        m_Path.Clear();

        //GameObject m_PathSelectorFather = PathSelector.gameObject.transform.parent.gameObject;

        //m_PathSelectorFatherList.Add(m_PathSelectorFather);


        for (int i = 0; i < PathSelector.transform.childCount; i++)
        {
            GameObject child = PathSelector.transform.GetChild(i).gameObject;

            if(child.CompareTag("PathBifurcation"))
            {
                // elegir aleatoriamente un hijo y añadir los paths
                GameObject pathBifurcationChosen = child.transform.GetChild(Random.Range(0, child.transform.childCount)).gameObject;

                for (int m = 0; m < pathBifurcationChosen.transform.childCount; m++)
                {
                    GameObject childBif = pathBifurcationChosen.transform.GetChild(m).gameObject;

                    m_Path.Add(childBif.transform);
                }
            }
            else
            {
                m_Path.Add(child.gameObject.transform);
            }
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
