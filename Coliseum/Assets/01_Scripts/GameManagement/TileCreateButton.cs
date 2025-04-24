using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCreateButton : MonoBehaviour
{
    public HexPathCreator.PathDetails m_ThisWay;
    public HexPathCreator HexPath;
    // Start is called before the first frame update
    void Start()
    {
        HexPath = GameObject.FindWithTag("GameController").gameObject.GetComponent<HexPathCreator>();
    }
    public void CreateTheWay()
    {
        HexPath.SelectTiles(m_ThisWay);
    }
    public void SetTheWay(HexPathCreator.PathDetails theWay)
    {
        m_ThisWay = theWay;
    }
    //SelectTiles(m_PathList[SelectedPath]);
}
