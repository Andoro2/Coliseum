using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TowerCreator : MonoBehaviour
{
    public GameObject m_TurretSketch, m_TurretToBuild;
    private GameObject InstancedSketch;
    public LayerMask BuildableLayer;
    public Material WrongPlacement, CorrectPlacement;

    public bool OnGround = false, OverTowers = false;
    void Start()
    {

    }
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, BuildableLayer)) // si pega en la capa que toca
        {

            Vector3 targetPos = GridAdjust(hit.point);

            if(InstancedSketch == null)
            {
                InstancedSketch = Instantiate(m_TurretSketch, targetPos, Quaternion.identity);
            }
            else
            {// if not on ground or colliding with other towers

                OnGround = CheckIsOnGround(InstancedSketch);
                OverTowers = CheckIsOverlapWithTowers(InstancedSketch.transform);

                if (!OnGround || OverTowers)
                {
                    InstancedSketch.transform.GetChild(0).GetComponent<Renderer>().material = WrongPlacement;
                }
                else
                {
                    InstancedSketch.transform.GetChild(0).GetComponent<Renderer>().material = CorrectPlacement;
                }

                InstancedSketch.transform.position = targetPos;
            }

            if (Input.GetMouseButtonDown(0) && OnGround && !OverTowers && InstancedSketch != null)
            {
                Instantiate(m_TurretToBuild, targetPos, Quaternion.identity);
                if(!Input.GetKey(KeyCode.LeftShift))
                {
                    Destroy(InstancedSketch);
                    GetComponent<TowerCreator>().enabled = false;
                }
            }
        }
        else
        {
            if (InstancedSketch != null)
            {
                Destroy(InstancedSketch);
                InstancedSketch = null;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if(InstancedSketch != null) Destroy(InstancedSketch);
            GetComponent<TowerCreator>().enabled = false;
        }

        Vector3 GridAdjust(Vector3 hitPos)
        {
            float x = hitPos.x;
            float z = hitPos.z;

            float altoHex = Mathf.Sqrt(3) / 2;
            float desplazamientoX = 0.5f;

            int col = Mathf.RoundToInt(x);
            int row = Mathf.RoundToInt(z / altoHex);

            float xFinal = col;
            float zFinal = row * altoHex;

            if (row % 2 != 0)
            {
                xFinal += desplazamientoX;
            }

            return new Vector3(xFinal, 2f, zFinal);
        }
    }
    public void SetTowerToBuild(GameObject thisTurret)
    {
        Debug.Log("Torreta asignada");
        m_TurretToBuild = thisTurret;
    }
    public bool CheckIsOverlapWithTowers(Transform tower)
    {
        Vector3 center = tower.position;

        float radius = 0.85f; // truangle's heigth

        Collider[] overlappingColliders = Physics.OverlapCapsule(center + Vector3.up, center - Vector3.up, radius);

        foreach (Collider col in overlappingColliders)
        {
            if (col.gameObject != gameObject && col.CompareTag("Turret"))
            {
                return true; // overlap detected
            }
        }

        return false; // no overlap
    }
    public bool CheckIsOnGround(GameObject TowerSketch)
    {
        float checkRadius = 0.5f;
        foreach (Transform child in TowerSketch.transform.GetChild(1))
        {
            bool inTerrain = false;
            Collider[] m_Intersecting = Physics.OverlapSphere(child.transform.position, checkRadius);
            foreach (Collider c in m_Intersecting)
            {
                if (c.CompareTag("Constructable"))
                {
                    inTerrain = true;
                    break; // Todos las esquinas entran en terreno
                }
            }
            if (!inTerrain)
            {
                return false;
            }
        }               

        return true; // Alguna queda fuera
    }
}
