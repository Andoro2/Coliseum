using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static HexPathCreator;

public class EnemySpawner : MonoBehaviour
{
    #region EnemySpawn 
    public List<PathSpawn> m_PathSpawns = new List<PathSpawn>();
    public GameObject SpawnPoint;
    public List<GameObject> m_EnemyTypes = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnEnemy", 1f, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnEnemy();
        }
    }
    void SpawnEnemy()
    {
        foreach (PathSpawn spawnPoint in m_PathSpawns)
        {
            GameObject enemy = Instantiate(m_EnemyTypes[0], spawnPoint.m_PathSpawnPoint.transform.position, Quaternion.identity);
            enemy.GetComponent<EnemyMovement>().AssignPath(spawnPoint.m_PathSpawnPoint);
        }
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
                //Debug.Log("Nuevo path creado: " + nuevo.ID);
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
    public static class Type
    {
        public static Types Element = Types.Normal;
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
