using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    static GameObject m_SpawnPoint;
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
        SpawnPoint = m_SpawnPoint;
    }
    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(m_EnemyTypes[0], m_SpawnPoint.transform.position, Quaternion.identity);
        enemy.GetComponent<EnemyMovement>().AssignPath(m_SpawnPoint);
    }
    public void SetNewSpawnPoint(GameObject NewSpawnPoint)
    {
        m_SpawnPoint = NewSpawnPoint;
    }
}
