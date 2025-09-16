using System.Collections;
using System.Collections.Generic;
//using UnityEditor.UIElements;
using UnityEngine;
using Unity.Netcode;
using static HexPathCreator;

public class TileElementAsigned : NetworkBehaviour
{
    public enum TileElements { Null, Fire, Ice, Lightning, Water }
    public TileElements TileElement = TileElements.Null;

    public void AssignElement(ElementData TileElementData)
    {
        TileElement = TileElementData.Element;

        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = TileElementData.TileMat;
    }

    [ClientRpc]
    public void AssignElementClientRpc(int elementIndex)
    {
        //AssignElement(TileElementsData[elementIndex]);
    }
}
