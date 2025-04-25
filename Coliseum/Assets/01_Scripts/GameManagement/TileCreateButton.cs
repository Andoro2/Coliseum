using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCreateButton : MonoBehaviour
{
    public HexPathCreator.PathDetails m_ThisWay;
    public HexPathCreator HexPath;
    private GameObject papu;
    // Start is called before the first frame update
    void Start()
    {
        papu = GameObject.FindWithTag("GameController").gameObject;
        HexPath = papu.GetComponent<HexPathCreator>();
    }
    public void CreateTheWay()
    {
        HexPath.SelectTiles(m_ThisWay);
        papu.GetComponent<GameManager>().NextWave();
    }
    public void SetTheWay(HexPathCreator.PathDetails theWay)
    {
        m_ThisWay = theWay;
    }
    //SelectTiles(m_PathList[SelectedPath]);
}
