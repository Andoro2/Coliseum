using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Netcode;
public class SetTowerToBuild : MonoBehaviour
{
    public GameObject m_Tugget, m_GameController;
    public TowerCreator TurretMaker;
    // Start is called before the first frame update
    void Start()
    {
        m_GameController = GameObject.FindWithTag("GameController");
        TurretMaker = m_GameController.transform.GetComponent<TowerCreator>();
    }
    public void SetTurretToBeBuilded()
    {
        TurretMaker.enabled = true;
        TurretMaker.SetTowerToBuild(m_Tugget);
        //SetTurretToBuildTurretServerRpc();
    }
    /*[ServerRpc(RequireOwnership = false)]
    private void SetTurretToBuildTurretServerRpc()
    {
        TurretMaker.enabled = true;
        TurretMaker.SetTowerToBuild(m_Tugget);
    }*/
}
