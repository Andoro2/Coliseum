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

    public void SetTheWay(HexPathCreator.PathDetails way)
    {
        m_ThisWay = way;
    }
    //SelectTiles(m_PathList[SelectedPath]);
}
//NetworkObject NextTileChecker = Instantiate(m_NextTileChecker, tileCheckPosition, Quaternion.Euler(0, tileCheckYRotation, 0));
//m_PathList[pathIndex].m_NextTileChecker = NextTileChecker;
//path.m_NextTileChecker = Instantiate(m_NextTileChecker, m_PathStartPositions[i], Quaternion.Euler(0, yRotation, 0));
//m_PathList[pathIndex].m_NextTileChecker.name = "TileChecker_" + m_PathList[pathIndex].ID;