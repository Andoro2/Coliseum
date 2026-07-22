using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static HexPathCreator;

public class EnemySpawner : MonoBehaviour
{
    #region EnemySpawn 
    public List<PathSpawn> m_PathSpawns = new List<PathSpawn>();
    public static List<PathSpawn> m_StaticPathSpawns = new List<PathSpawn>();
    public GameObject SpawnPoint;
    public List<GameObject> m_EnemyList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        if(m_PathSpawns.Count > 0) InvokeRepeating("SpawnEnemyServerRpc", 1f, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        m_StaticPathSpawns = m_PathSpawns;
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (m_PathSpawns.Count > 0) SpawnEnemy();
        }
    }
    private void SpawnEnemy()
    {
        for (int p = 0; p < m_PathSpawns.Count; p++)
        {
            GameObject enemy = Instantiate(m_EnemyList[Random.Range(0, m_EnemyList.Count)], m_PathSpawns[p].m_PathSpawnPoint.transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyMovement>().AssignPath(m_PathSpawns[p].m_PathSpawnPoint);


            //NetworkObject EnemyNetworkObject = enemy.GetComponent<NetworkObject>();
            //EnemyNetworkObject.Spawn(true);
            //EnemyNetworkObject.GetComponent<EnemyMovement>().AssignPathClientRpc(p);

        }

        /*foreach (PathSpawn spawnPoint in m_PathSpawns)
        {
            GameObject enemy = Instantiate(m_EnemyTypes[Random.Range(0,m_EnemyTypes.Count)], spawnPoint.m_PathSpawnPoint.transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyMovement>().AssignPath(spawnPoint.m_PathSpawnPoint);


            NetworkObject EnemyNetworkObject = enemy.GetComponent<NetworkObject>();
            EnemyNetworkObject.Spawn(true);
            //EnemyNetworkObject.GetComponent<EnemyMovement>().AssignPathClientRpc(int pathIndex);
        }*/

    }

    public void UpdatePathList(PathDetails path, GameObject tile)
    {
        PathSpawn existente = m_PathSpawns.FirstOrDefault(spwn => spwn.ID == path.ID);

        if (existente != null) // Ya existe el path
        {
            //Debug.Log("Existe el path: " + path.ID);
            GameObject spwnP = GetNextAvailablePath(tile);
            if (spwnP != null)
                existente.m_PathSpawnPoint = spwnP;
        }
        else
        {
            GameObject spwnP = GetNextAvailablePath(tile);
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

                bool used = m_PathSpawns.Any(p => p.m_PathSpawnPoint.transform.position == posible.transform.position);
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
    #endregion

    #region Elemental System

    public enum Types
    {
        Normal,
        Fire,
        Ice,
        Wind,
        Tech,
        Earth,
        Blood,
        Lightning
    }

    public enum States
    {
        Null,
        Slow,
        Stun,
        Burn,
        Wet,
        Blood
    }
    public static class Stage
    {
        public static States Element = States.Null;
    }

    #endregion
}
