using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static EnemySpawner;

public class EnemyMovement : NetworkBehaviour
{
    public float m_Speed = 5f;
    public List<Transform> m_Path = new List<Transform>();
    public List<GameObject> m_TurretsTargetedBy = new List<GameObject>();
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
            foreach(GameObject turret in m_TurretsTargetedBy)
            {
                if (turret.GetComponent<InRangeManager>().enemiesInRange.Contains(gameObject))
                {
                    turret.GetComponent<InRangeManager>().RemoveFromList(gameObject);
                }
            }


            Debug.Log("DED");
            GameObject.FindWithTag("GameController").GetComponent<GameManager>().TakeDamage(1);

            Destroy(gameObject);
        }
        if (other.CompareTag("TurretRange"))
        {
            m_TurretsTargetedBy.Add(other.gameObject);
        }
    }
    [ClientRpc]
    public void AssignPathClientRpc(int pathIndex)
    {
        List<PathSpawn> PathSpawns = new List<PathSpawn>();
        PathSpawns = m_StaticPathSpawns;
        m_Path.Clear();

        for (int i = 0; i < PathSpawns[pathIndex].m_PathSpawnPoint.transform.childCount; i++)
        {
            GameObject child = PathSpawns[pathIndex].m_PathSpawnPoint.transform.GetChild(i).gameObject;

            if (child.CompareTag("PathBifurcation"))
            {
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
    public void AssignPath(GameObject PathSelector)
    {
        m_Path.Clear();

        for (int i = 0; i < PathSelector.transform.childCount; i++)
        {
            GameObject child = PathSelector.transform.GetChild(i).gameObject;

            if(child.CompareTag("PathBifurcation"))
            {
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
}
