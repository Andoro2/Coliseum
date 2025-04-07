using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static HexPathCreator;

public class EnemySpawner : MonoBehaviour
{
    public List<PathSpawn> m_PathSpawns = new List<PathSpawn>();
    //static GameObject m_SpawnPoint;
    public GameObject SpawnPoint;
    public List<GameObject> m_EnemyTypes = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnEnemy", 1f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        //SpawnPoint = m_SpawnPoint;
    }
    void SpawnEnemy()
    {
        foreach (PathSpawn spawnPoint in m_PathSpawns)
        {
            GameObject enemy = Instantiate(m_EnemyTypes[0], spawnPoint.m_PathSpawnPoint.transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyMovement>().AssignPath(spawnPoint.m_PathSpawnPoint);
        }
    }
    /*public void SetNewSpawnPoint(GameObject NewSpawnPoint)
    {
        m_SpawnPoint = NewSpawnPoint;
    }*/
    public void UpdatePathList(PathDetails path, GameObject tile)
    {
        GameObject spwnP = tile.transform.Find("Path0").gameObject;
        if (m_PathSpawns.Any(spwn => spwn.ID == path.ID)) // si ya existe el path
        {
            m_PathSpawns.FirstOrDefault(p => p.ID == path.ID).m_PathSpawnPoint = spwnP;
        }
        else
        {
            spwnP = GetNextAvailablePath(tile);
            if (spwnP != null)
            {
                PathSpawn nuevo = new PathSpawn
                {
                    ID = path.ID,
                    m_PathSpawnPoint = spwnP
                };
                m_PathSpawns.Add(nuevo);
            }
        }
    }
    private GameObject GetNextAvailablePath(GameObject tile)
    {
        for (int i = 0; i < tile.transform.childCount; i++)
        {
            string nombre = $"Path{i}";
            Transform hijo = tile.transform.Find(nombre);

            if (hijo != null)
            {
                GameObject posible = hijo.gameObject;

                bool used = m_PathSpawns.Any(p => p.m_PathSpawnPoint == posible);
                if (!used)
                    return posible;
            }
        }
        return null;
    }
    [System.Serializable]
    public class PathSpawn
    {
        public string ID = "path_";
        public GameObject m_PathSpawnPoint;
    }
}
