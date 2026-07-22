using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class SetTowerToBuild : NetworkBehaviour
{
    public int m_TurretIndex;
    public TowerCreator TurretMaker;
    void Start()
    {
        TurretMaker = GameObject.FindWithTag("GameController").GetComponent<TowerCreator>();
    }
    public void SetTurretToBeBuilded()
    {
        TurretMaker.enabled = true;
        TurretMaker.SetTowerIndex(m_TurretIndex);
    }
}
