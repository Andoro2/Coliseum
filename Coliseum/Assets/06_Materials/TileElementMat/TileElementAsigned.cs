using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using static HexPathCreator;

public class TileElementAsigned : MonoBehaviour
{
    public enum TileElements { Null, Fire, Ice, Lightning, Water }
    public TileElements TileElement = TileElements.Null;

    public void AssignElement(ElementData TileElementData)
    {
        TileElement = TileElementData.Element;

        transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = TileElementData.TileMat;
    }
}
