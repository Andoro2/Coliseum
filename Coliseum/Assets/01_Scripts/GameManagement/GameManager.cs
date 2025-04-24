using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsFighting;
    private List<GameObject> tileCanvases = new List<GameObject>();

    void Update()
    {
        // Buscar objetos activos con el tag
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("CreateTileCanvas");

        // Añadir nuevos objetos a la lista si no estaban ya
        foreach (GameObject obj in foundObjects)
        {
            if (!tileCanvases.Contains(obj))
            {
                tileCanvases.Add(obj);
            }
        }

        // Activar o desactivar según el estado de IsFighting
        foreach (GameObject obj in tileCanvases)
        {
            if (obj == null) continue; // Puede haber sido destruido

            if (IsFighting && obj.activeSelf)
            {
                obj.SetActive(false);
            }
            else if (!IsFighting && !obj.activeSelf)
            {
                obj.SetActive(true);
            }
        }
    }
}
