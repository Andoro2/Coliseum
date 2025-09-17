using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
//using UnityEditor.UIElements;
using UnityEngine;
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
    public void AssignElementClientRpc(int MaterialIndex)
    {
        //AssignElement(TileElementsData[elementIndex]);

        TileElement = HexPathCreator.StaticTileElementsData[MaterialIndex].Element;

        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = HexPathCreator.StaticTileElementsData[MaterialIndex].TileMat;
    }
}
