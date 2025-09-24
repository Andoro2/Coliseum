using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static HexPathCreator;

public class TowerCreator : NetworkBehaviour
{
    public GameObject m_TurretSketch, m_TurretToBuild;
    private GameObject InstancedTurretSketch;
    public LayerMask BuildableLayer;
    public Material WrongPlacement, CorrectPlacement;

    public bool OnGround = false, OverTowers = false;

    [ServerRpc(RequireOwnership = false)]
    private void SpawnTurretServerRpc(Vector3 turretPos)
    {
        GameObject turret = Instantiate(m_TurretToBuild, turretPos, Quaternion.identity);
        //InstancedTurretSketch = turret;

        NetworkObject TileNetworkObject = turret.GetComponent<NetworkObject>();
        TileNetworkObject.Spawn(true);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroySketchTurretServerRpc()
    {
        NetworkObject TileNetworkObject = InstancedTurretSketch.GetComponent<NetworkObject>();
        TileNetworkObject.Despawn(true);
        InstancedTurretSketch = null;
        Destroy(InstancedTurretSketch);
    }
    void Update()
    {
        //if (!IsOwner) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, BuildableLayer)) // si pega en la capa que toca
        {

            Vector3 targetPos = GridAdjust(hit.point);
            targetPos += Vector3.down;
            if(InstancedTurretSketch == null)
            {
                //SpawnTurretServerRpc(targetPos, true);
                InstancedTurretSketch = Instantiate(m_TurretSketch, targetPos, Quaternion.identity);
            }
            else
            {// if not on ground or colliding with other towers

                OnGround = CheckIsOnGround(InstancedTurretSketch);
                OverTowers = CheckIsOverlapWithTowers(InstancedTurretSketch.transform);

                if (!OnGround || OverTowers)
                {
                    InstancedTurretSketch.transform.GetChild(0).transform.GetChild(1).GetComponent<Renderer>().material = WrongPlacement;
                }
                else
                {
                    InstancedTurretSketch.transform.GetChild(0).transform.GetChild(1).GetComponent<Renderer>().material = CorrectPlacement;
                }

                InstancedTurretSketch.transform.position = targetPos;
            }

            if (Input.GetMouseButtonDown(0) && OnGround && !OverTowers && InstancedTurretSketch != null
                && GetComponent<GameManager>().m_Currency >= m_TurretToBuild.GetComponent<TowerStats>().m_Cost)
            {
                //Instantiate(m_TurretToBuild, targetPos, Quaternion.identity);
                //DestroySketchTurretServerRpc();
                SpawnTurretServerRpc(targetPos);

                GetComponent<GameManager>().SpendMoney(m_TurretToBuild.GetComponent<TowerStats>().m_Cost);

                Destroy(InstancedTurretSketch);
                GetComponent<TowerCreator>().enabled = false;

                /*if (!Input.GetKey(KeyCode.LeftShift))
                {
                    //DestroySketchTurretServerRpc();
                    Destroy(InstancedTurretSketch);
                    GetComponent<TowerCreator>().enabled = false;
                }*/
            }
        }
        else
        {
            if (InstancedTurretSketch != null)
            {
                Destroy(InstancedTurretSketch);
                InstancedTurretSketch = null;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if(InstancedTurretSketch != null) Destroy(InstancedTurretSketch);
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
        //Debug.Log("Torreta asignada");
        m_TurretToBuild = thisTurret;
    }
    public bool CheckIsOverlapWithTowers(Transform tower)
    {
        Vector3 center = tower.position;

        float radius = 0.85f; // truangle's heigth

        Collider[] overlappingColliders = Physics.OverlapCapsule(center + Vector3.up, center - Vector3.up, radius);

        foreach (Collider col in overlappingColliders)
        {
            //if (col.gameObject != gameObject && col.CompareTag("Turret"))
            if (col.CompareTag("Turret"))
            {
                return true; // overlap detected
            }
        }

        return false; // no overlap
    }
    public bool CheckIsOnGround(GameObject TowerSketch)
    {
        float checkRadius = 0.05f;
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
