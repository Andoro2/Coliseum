using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRangeManager : MonoBehaviour
{
    public List<GameObject> enemiesInRange = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other.gameObject))
                enemiesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other.gameObject))
                enemiesInRange.Remove(other.gameObject);
        }
    }
    public void RemoveFromList(GameObject obj)
    {
        if (enemiesInRange.Contains(obj))
        {
            enemiesInRange.Remove(obj);
        }
    }
}
