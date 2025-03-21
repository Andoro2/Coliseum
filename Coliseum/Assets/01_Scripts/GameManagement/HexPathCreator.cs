using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HexPathCreator : MonoBehaviour
{
    public enum HexOrientation { Zero, One, Two, Three, Four, Five, End }
    public List<HexTileDetails> m_HexTiles = new List<HexTileDetails>();

    public List<PathDetails> m_PathList = new List<PathDetails>();

    private Vector2 StartPathValues = new Vector2(45, 30 * Mathf.Sqrt(3));
    public bool m_PathZero, m_PathOne, m_PathTwo, m_PathThree, m_PathFour, m_PathFive;

    public GameObject m_TileContainer, m_PathHolder, m_NextTileChecker;

    void Start()
    {
        List<Vector3> m_PathStartPositions = new List<Vector3>
        {
            new Vector3(0, 0, -StartPathValues.y),                      //path zero
            new Vector3(-StartPathValues.x, 0, -StartPathValues.y / 2), //path one
            new Vector3(-StartPathValues.x, 0, StartPathValues.y / 2),  //path two
            new Vector3(0, 0, StartPathValues.y),                       //path three
            new Vector3(StartPathValues.x, 0, StartPathValues.y / 2),   //path four
            new Vector3(StartPathValues.x, 0, -StartPathValues.y / 2)   //path five
        };

        List<HexTileDetails> InitialTiles = new List<HexTileDetails>();
        InitialTiles.AddRange(m_HexTiles.Where(p => !p.Bifurcations.Exit_1 && p.Bifurcations.Exit_2 && !p.Bifurcations.Exit_3 && !p.Bifurcations.Exit_4 && !p.Bifurcations.Exit_5 && !p.Bifurcations.End && !p.DoubleTile));
        InitialTiles.AddRange(m_HexTiles.Where(p => !p.Bifurcations.Exit_1 && !p.Bifurcations.Exit_2 && p.Bifurcations.Exit_3 && !p.Bifurcations.Exit_4 && !p.Bifurcations.Exit_5 && !p.Bifurcations.End && !p.DoubleTile));
        InitialTiles.AddRange(m_HexTiles.Where(p => !p.Bifurcations.Exit_1 && !p.Bifurcations.Exit_2 && !p.Bifurcations.Exit_3 && p.Bifurcations.Exit_4 && !p.Bifurcations.Exit_5 && !p.Bifurcations.End && !p.DoubleTile));

        bool[] paths = { m_PathZero, m_PathOne, m_PathTwo, m_PathThree, m_PathFour, m_PathFive };
        HexOrientation[] beginningPathsOrientations = { HexOrientation.Zero, HexOrientation.One, HexOrientation.Two,
            HexOrientation.Three, HexOrientation.Four, HexOrientation.Five };

        for (int i = 0; i < paths.Length; i++)
        {
            if (paths[i])
            {
                PathDetails path = new PathDetails();
                path.ID = path.ID + m_PathList.Count();
                path.PathOrientation = beginningPathsOrientations[i];
                path.m_PathNextPosition = m_PathStartPositions[i];


                GameObject PathContainer = Instantiate(m_PathHolder, path.m_PathNextPosition, Quaternion.identity);
                path.m_Container = PathContainer;

                int yRotation = adjustRotation(path) % 360;
                path.m_NextTileChecker = Instantiate(m_NextTileChecker, m_PathStartPositions[i], Quaternion.Euler(0, yRotation, 0));
                path.m_NextTileChecker.name = "TileChecker_" + path.ID;

                PathContainer.name = path.ID;

                m_PathList.Add(path);

                BuildTrack(path, InitialTiles, m_PathStartPositions[i]);
            }
        }
        InvokeRepeating("AutoGenerate", 0f, 0.1f);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
        if (Input.GetKeyDown(KeyCode.Space) && m_PathList.Count > 0)
        {
            int SelectedPath = Random.Range(0, m_PathList.Count);
            while (!m_PathList[SelectedPath].m_PathActive)
            {
                SelectedPath = Random.Range(0, m_PathList.Count);
            }
            if (m_PathList[SelectedPath].m_PathActive)
            {
                SelectTiles(m_PathList[SelectedPath]);
            }
        }
    }
    public void PathBifurcation(PathDetails PrevPath, int newOrientation, GameObject GeneratedTile)
    {
        PathDetails path = new PathDetails();

        m_PathList.Add(path);

        path.ID = "path_" + (m_PathList.Count() - 1);

        path.m_PathNextPosition = PrevPath.m_PathNextPosition;

        path.m_Container = Instantiate(PrevPath.m_Container, PrevPath.m_Container.transform.position, Quaternion.identity);
        path.m_Container.name = path.ID;

        foreach (Transform child in path.m_Container.transform)
        {
            Destroy(child.gameObject);
        }

        switch (newOrientation)
        {
            // close turn left
            case 1:
            case 6:
            case 8:
            case 10:
                path.PathOrientation = CloseTurnLeft(PrevPath.PathOrientation); break;
            // long turn left
            case 2:
            case 9:
            case 11:
            case 13:
                path.PathOrientation = LongTurnLeft(PrevPath.PathOrientation); break;
            // straight
            case 3:
            case 12:
            case 14:
            case 16:
                path.PathOrientation = PrevPath.PathOrientation; break;
            // long turn right
            case 4:
            case 15:
            case 17:
            case 19:
                path.PathOrientation = LongTurnRight(PrevPath.PathOrientation); break;
            // close turn right
            case 5:
            case 18:
            case 20:
            case 22:
                path.PathOrientation = CloseTurnRight(PrevPath.PathOrientation); break;
            // u turn
            case 7:
            case 21:
                path.PathOrientation = UTurn(PrevPath.PathOrientation); break;
        }

        path.m_NextTileChecker = Instantiate(PrevPath.m_NextTileChecker, GeneratedTile.transform.position + UpdateNextTilePos(path, newOrientation), Quaternion.identity);

        float yRotation = (adjustRotation(path) + 360) % 360;
        path.m_NextTileChecker.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        path.m_NextTileChecker.name = "TileChecker_" + path.ID;

        path.m_PathNextPosition += UpdateNextTilePos(path, newOrientation);
    }    
    void AutoGenerate()
    {
        if (m_PathList.Count > 0)
        {
            int SelectedPath = Random.Range(0, m_PathList.Count);
            SelectTiles(m_PathList[SelectedPath]);
        }
        else
        {
            CancelInvoke("AddPath");
        }
    }
    public void SelectTiles(PathDetails Track)
    {
        List<HexTileDetails> m_AvailableTracks = new List<HexTileDetails>();

        HashSet<int> m_AvailableExits = new HashSet<int>();

        Transform NextTileChecker = Track.m_NextTileChecker.transform, 
            DoubleTileChecker = NextTileChecker.transform.Find("DoubleTileChecker").transform;

        bool DoubleTileFits = true;

        if (CheckPosition(NextTileChecker.Find("ExitOne").position))   m_AvailableExits.Add(1);
        if (CheckPosition(NextTileChecker.Find("ExitTwo").position)) m_AvailableExits.Add(2);
        else DoubleTileFits = false;
        if (CheckPosition(NextTileChecker.Find("ExitThree").position)) m_AvailableExits.Add(3);
        else DoubleTileFits = false;
        if (CheckPosition(NextTileChecker.Find("ExitFour").position))  m_AvailableExits.Add(4);
        else DoubleTileFits = false;
        if (CheckPosition(NextTileChecker.Find("ExitFive").position))  m_AvailableExits.Add(5);

        if (!CheckPosition(NextTileChecker.Find("DoubleTileCheck1").position)) DoubleTileFits = false;
        if (!CheckPosition(NextTileChecker.Find("DoubleTileCheck2").position)) DoubleTileFits = false;
        if (!CheckPosition(NextTileChecker.Find("DoubleTileCheck3").position)) DoubleTileFits = false;

        // double tile checks if it fits
        if (DoubleTileFits)
        {
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_6").position))
            {
                m_AvailableExits.Add(6);
                m_AvailableExits.Add(7);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_7").position))
            {
                m_AvailableExits.Add(8);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_8").position))
            {
                m_AvailableExits.Add(9);
                m_AvailableExits.Add(10);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_9").position))
            {
                m_AvailableExits.Add(11);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_10").position))
            {
                m_AvailableExits.Add(12);
                m_AvailableExits.Add(13);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_11").position))
            {
                m_AvailableExits.Add(14);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_12").position))
            {
                m_AvailableExits.Add(15);
                m_AvailableExits.Add(16);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_13").position))
            {
                m_AvailableExits.Add(17);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_14").position))
            {
                m_AvailableExits.Add(18);
                m_AvailableExits.Add(19);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_15").position))
            {
                m_AvailableExits.Add(20);
            }
            if (CheckPosition(DoubleTileChecker.Find("DoubleExit_16").position))
            {
                m_AvailableExits.Add(21);
                m_AvailableExits.Add(22);
            }
        }

        foreach (HexTileDetails tile in m_HexTiles)
        {
            HashSet<int> tileExits = new HashSet<int>();

            if (!tile.DoubleTile)
            {
                if (tile.Bifurcations.Exit_1) tileExits.Add(1);
                if (tile.Bifurcations.Exit_2) tileExits.Add(2);
                if (tile.Bifurcations.Exit_3) tileExits.Add(3);
                if (tile.Bifurcations.Exit_4) tileExits.Add(4);
                if (tile.Bifurcations.Exit_5) tileExits.Add(5);
            }
            else
            {
                if (tile.DoubleTileBifurcations.Exit_6)  tileExits.Add(6);
                if (tile.DoubleTileBifurcations.Exit_7)  tileExits.Add(7);
                if (tile.DoubleTileBifurcations.Exit_8)  tileExits.Add(8);
                if (tile.DoubleTileBifurcations.Exit_9)  tileExits.Add(9);
                if (tile.DoubleTileBifurcations.Exit_10) tileExits.Add(10);
                if (tile.DoubleTileBifurcations.Exit_11) tileExits.Add(11);
                if (tile.DoubleTileBifurcations.Exit_12) tileExits.Add(12);
                if (tile.DoubleTileBifurcations.Exit_13) tileExits.Add(13);
                if (tile.DoubleTileBifurcations.Exit_14) tileExits.Add(14);
                if (tile.DoubleTileBifurcations.Exit_15) tileExits.Add(15);
                if (tile.DoubleTileBifurcations.Exit_16) tileExits.Add(16);
                if (tile.DoubleTileBifurcations.Exit_17) tileExits.Add(17);
                if (tile.DoubleTileBifurcations.Exit_18) tileExits.Add(18);
                if (tile.DoubleTileBifurcations.Exit_19) tileExits.Add(19);
                if (tile.DoubleTileBifurcations.Exit_20) tileExits.Add(20);
                if (tile.DoubleTileBifurcations.Exit_21) tileExits.Add(21);
                if (tile.DoubleTileBifurcations.Exit_22) tileExits.Add(22);
            }

            if (tileExits.IsSubsetOf(m_AvailableExits) && tileExits.Count > 0)
            {
                m_AvailableTracks.Add(tile);
            }
        }

        if (m_AvailableTracks.Count == 0)
        {
            Debug.Log("No available tiles found, allowing ExitZero tiles.");

            foreach (HexTileDetails tile in m_HexTiles)
            {
                if (tile.Bifurcations.End)
                {
                    m_AvailableTracks.Add(tile);
                }
            }
        }

        BuildTrack(Track, m_AvailableTracks, Track.m_PathNextPosition);
    }

    public void BuildTrack(PathDetails Path, List<HexTileDetails> m_AvailablePaths, Vector3 NextTilePos)
    {
        List<PathDetails> NewPaths = new List<PathDetails>();

        if (m_AvailablePaths.Count == 0 || Path.PathOrientation == HexOrientation.End)
        {
            Debug.Log("No hay caminos disponibles, fin del camino.");
            return;
        }
     
        HexTileDetails Tile = m_AvailablePaths[Random.Range(0, m_AvailablePaths.Count)];

        if (Tile.DoubleTile)
        {
            switch (Path.PathOrientation)
            {
                case HexOrientation.Zero:
                    NextTilePos.z -= (10 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.One:
                    NextTilePos.x -= 15;
                    NextTilePos.z -= (5 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Two:
                    NextTilePos.x -= 15;
                    NextTilePos.z += (5 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Three:
                    NextTilePos.z += (10 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Four:
                    NextTilePos.x += 15;
                    NextTilePos.z += (5 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Five:
                    NextTilePos.x += 15;
                    NextTilePos.z -= (5 * Mathf.Sqrt(3));
                    break;
            }
        }

        GameObject GeneratedTile = Instantiate(Tile.HexTile, NextTilePos, Quaternion.identity);

        float yRotation = adjustRotation(Path) % 360;
        GeneratedTile.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        int bifurcation = 0;
        if (CheckDivisions(Tile) > 1)
        {
            int lastBifurcation = 0;

            if (Tile.Bifurcations.Exit_1) lastBifurcation = NewPather(1, lastBifurcation);
            if (Tile.Bifurcations.Exit_2) lastBifurcation = NewPather(2, lastBifurcation);
            if (Tile.Bifurcations.Exit_3) lastBifurcation = NewPather(3, lastBifurcation);
            if (Tile.Bifurcations.Exit_4) lastBifurcation = NewPather(4, lastBifurcation);
            if (Tile.Bifurcations.Exit_5) lastBifurcation = NewPather(5, lastBifurcation);

            if (Tile.DoubleTile)
            {
                if (Tile.DoubleTileBifurcations.Exit_6)  lastBifurcation = NewPather( 6, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_7)  lastBifurcation = NewPather( 7, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_8)  lastBifurcation = NewPather( 8, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_9)  lastBifurcation = NewPather( 9, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_10) lastBifurcation = NewPather(10, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_11) lastBifurcation = NewPather(11, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_12) lastBifurcation = NewPather(12, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_13) lastBifurcation = NewPather(13, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_14) lastBifurcation = NewPather(14, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_15) lastBifurcation = NewPather(15, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_16) lastBifurcation = NewPather(16, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_17) lastBifurcation = NewPather(17, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_18) lastBifurcation = NewPather(18, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_19) lastBifurcation = NewPather(19, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_20) lastBifurcation = NewPather(20, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_21) lastBifurcation = NewPather(21, lastBifurcation);
                if (Tile.DoubleTileBifurcations.Exit_22) lastBifurcation = NewPather(22, lastBifurcation);
            }

            if (lastBifurcation != 0) // bifurcation type end
            {
                UpdatePath(lastBifurcation, Path, GeneratedTile);
            }
        }
        else
        {
            if (Tile.Bifurcations.Exit_1) bifurcation = 1;
            if (Tile.Bifurcations.Exit_2) bifurcation = 2;
            if (Tile.Bifurcations.Exit_3) bifurcation = 3;
            if (Tile.Bifurcations.Exit_4) bifurcation = 4;
            if (Tile.Bifurcations.Exit_5) bifurcation = 5;
            if (Tile.Bifurcations.End)    bifurcation = 0;

            if (Tile.DoubleTile)
            {
                if (Tile.DoubleTileBifurcations.Exit_6)  bifurcation = 6;
                if (Tile.DoubleTileBifurcations.Exit_7)  bifurcation = 7;
                if (Tile.DoubleTileBifurcations.Exit_8)  bifurcation = 8;
                if (Tile.DoubleTileBifurcations.Exit_9)  bifurcation = 9;
                if (Tile.DoubleTileBifurcations.Exit_10) bifurcation = 10;
                if (Tile.DoubleTileBifurcations.Exit_11) bifurcation = 11;
                if (Tile.DoubleTileBifurcations.Exit_12) bifurcation = 12;
                if (Tile.DoubleTileBifurcations.Exit_13) bifurcation = 13;
                if (Tile.DoubleTileBifurcations.Exit_14) bifurcation = 14;
                if (Tile.DoubleTileBifurcations.Exit_15) bifurcation = 15;
                if (Tile.DoubleTileBifurcations.Exit_16) bifurcation = 16;
                if (Tile.DoubleTileBifurcations.Exit_17) bifurcation = 17;
                if (Tile.DoubleTileBifurcations.Exit_18) bifurcation = 18;
                if (Tile.DoubleTileBifurcations.Exit_19) bifurcation = 19;
                if (Tile.DoubleTileBifurcations.Exit_20) bifurcation = 20;
                if (Tile.DoubleTileBifurcations.Exit_21) bifurcation = 21;
                if (Tile.DoubleTileBifurcations.Exit_22) bifurcation = 22;
            }

            UpdatePath(bifurcation, Path, GeneratedTile);
        }
        int NewPather(int bifurcation, int lastBifurcation)
        {
            if (lastBifurcation == 0) return bifurcation;
            else
            {
                PathBifurcation(Path, bifurcation, GeneratedTile);
                return lastBifurcation;
            }
        }
    }
    private void UpdatePath(int HexTileBifurcation, PathDetails Path, GameObject GeneratedTile)
    {
        if (HexTileBifurcation == 1
            || HexTileBifurcation == 6
            || HexTileBifurcation == 8
            || HexTileBifurcation == 10)
                Path.PathOrientation = CloseTurnLeft(Path.PathOrientation);
        if (HexTileBifurcation == 2
            || HexTileBifurcation == 9
            || HexTileBifurcation == 11
            || HexTileBifurcation == 13)
                Path.PathOrientation = LongTurnLeft(Path.PathOrientation);
        if (HexTileBifurcation == 4
            || HexTileBifurcation == 15
            || HexTileBifurcation == 17
            || HexTileBifurcation == 19)
                Path.PathOrientation = LongTurnRight(Path.PathOrientation);
        if (HexTileBifurcation == 5
            || HexTileBifurcation == 18
            || HexTileBifurcation == 20
            || HexTileBifurcation == 22)
                Path.PathOrientation = CloseTurnRight(Path.PathOrientation);
        if (HexTileBifurcation == 7
            || HexTileBifurcation == 21)
            Path.PathOrientation = UTurn(Path.PathOrientation);
        if (HexTileBifurcation == 0)
        {
            Path.PathOrientation = HexOrientation.End;
            GeneratedTile.gameObject.transform.SetParent(Path.m_Container.gameObject.transform);
            Destroy(Path.m_NextTileChecker);
            m_PathList.Remove(Path);
            return;
        }

        Path.m_PathNextPosition += UpdateNextTilePos(Path, HexTileBifurcation);

        Path.m_NextTileChecker.transform.position = Path.m_PathNextPosition;

        float yRotation = adjustRotation(Path) % 360;

        Path.m_NextTileChecker.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        GeneratedTile.gameObject.transform.SetParent(Path.m_Container.gameObject.transform);
    }
    private int adjustRotation(PathDetails camino)
    {
        int rotation = 0;
        switch (camino.PathOrientation)
        {
            case HexOrientation.Zero:
                rotation = 180;
                break;
            case HexOrientation.One:
                rotation = 240;
                break;
            case HexOrientation.Two:
                rotation = 300;
                break;
            case HexOrientation.Three:
                rotation = 0;
                break;
            case HexOrientation.Four:
                rotation = 60;
                break;
            case HexOrientation.Five:
                rotation = 120;
                break;
        }
        return rotation;
    }
    private int CheckDivisions(HexTileDetails Tile)
    {
        int exitCount = 0;

        if (Tile.Bifurcations.Exit_1) exitCount++;
        if (Tile.Bifurcations.Exit_2) exitCount++;
        if (Tile.Bifurcations.Exit_3) exitCount++;
        if (Tile.Bifurcations.Exit_4) exitCount++;
        if (Tile.Bifurcations.Exit_5) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_6) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_7) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_8) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_9) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_10) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_11) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_12) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_13) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_14) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_15) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_16) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_17) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_18) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_19) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_20) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_21) exitCount++;
        if (Tile.DoubleTileBifurcations.Exit_22) exitCount++;

        return exitCount;
    }
    private HexOrientation LongTurnRight(HexOrientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case HexOrientation.Zero: return HexOrientation.One;
            case HexOrientation.One: return HexOrientation.Two;
            case HexOrientation.Two: return HexOrientation.Three;
            case HexOrientation.Three: return HexOrientation.Four;
            case HexOrientation.Four: return HexOrientation.Five;
            case HexOrientation.Five: return HexOrientation.Zero;
            default: return currentOrientation;
        }
    }
    private HexOrientation CloseTurnRight(HexOrientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case HexOrientation.Zero: return HexOrientation.Two;
            case HexOrientation.One: return HexOrientation.Three;
            case HexOrientation.Two: return HexOrientation.Four;
            case HexOrientation.Three: return HexOrientation.Five;
            case HexOrientation.Four: return HexOrientation.Zero;
            case HexOrientation.Five: return HexOrientation.One;
            default: return currentOrientation;
        }
    }
    private HexOrientation LongTurnLeft(HexOrientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case HexOrientation.Zero: return HexOrientation.Five;
            case HexOrientation.One: return HexOrientation.Zero;
            case HexOrientation.Two: return HexOrientation.One;
            case HexOrientation.Three: return HexOrientation.Two;
            case HexOrientation.Four: return HexOrientation.Three;
            case HexOrientation.Five: return HexOrientation.Four;
            default: return currentOrientation;
        }
    }
    private HexOrientation CloseTurnLeft(HexOrientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case HexOrientation.Zero: return HexOrientation.Four;
            case HexOrientation.One: return HexOrientation.Five;
            case HexOrientation.Two: return HexOrientation.Zero;
            case HexOrientation.Three: return HexOrientation.One;
            case HexOrientation.Four: return HexOrientation.Two;
            case HexOrientation.Five: return HexOrientation.Three;
            default: return currentOrientation;
        }
    }
    private HexOrientation UTurn(HexOrientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case HexOrientation.Zero: return HexOrientation.Three;
            case HexOrientation.One: return HexOrientation.Four;
            case HexOrientation.Two: return HexOrientation.Five;
            case HexOrientation.Three: return HexOrientation.Zero;
            case HexOrientation.Four: return HexOrientation.One;
            case HexOrientation.Five: return HexOrientation.Two;
            default: return currentOrientation;
        }
    }
    private Vector3 UpdateNextTilePos(PathDetails PathOrientation, int BifurcationNumber)
    {
        Debug.Log(BifurcationNumber);
        Vector3 NewPos = new Vector3();
        List<Vector3> TileNewPos = new List<Vector3>
        {
            new Vector3(-15, 0, -10 * Mathf.Sqrt(3)), //  6  7
            new Vector3(-30, 0,                   0), //  8
            new Vector3(-30, 0,  10 * Mathf.Sqrt(3)), //  9 10
            new Vector3(-30, 0,  15 * Mathf.Sqrt(3)), // 11
            new Vector3(-15, 0,  25 * Mathf.Sqrt(3)), // 12 13
            new Vector3(  0, 0,  60 * Mathf.Sqrt(3)), // 14
            new Vector3( 15, 0,  25 * Mathf.Sqrt(3)), // 15 16
            new Vector3( 30, 0,  15 * Mathf.Sqrt(3)), // 17
            new Vector3( 30, 0,  10 * Mathf.Sqrt(3)), // 18 19
            new Vector3( 30, 0,                   0), // 20
            new Vector3( 15, 0, -10 * Mathf.Sqrt(3)), // 21 22
        };
        if(BifurcationNumber < 6)
        {
            switch (PathOrientation.PathOrientation)
            {
                case HexOrientation.Zero:
                    NewPos.z -= (10 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.One:
                    NewPos.x -= 15;
                    NewPos.z -= (5 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Two:
                    NewPos.x -= 15;
                    NewPos.z += (5 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Three:
                        NewPos.z += (10 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Four:
                    NewPos.x += 15;
                    NewPos.z += (5 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Five:
                    NewPos.x += 15;
                    NewPos.z -= (5 * Mathf.Sqrt(3));
                    break;
            }
        }
        else
        {
            switch (PathOrientation.PathOrientation)
            {
                case HexOrientation.Zero:
                    NewPos.z -= (30 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.One:
                    NewPos.x -= 45;
                    NewPos.z -= (15 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Two:
                    NewPos.x -= 45;
                    NewPos.z += (15 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Three:
                    NewPos.z += (30 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Four:
                    NewPos.x += 45;
                    NewPos.z += (15 * Mathf.Sqrt(3));
                    break;
                case HexOrientation.Five:
                    NewPos.x += 45;
                    NewPos.z -= (15 * Mathf.Sqrt(3));
                    break;
            }
        }
        return NewPos;
    }
    [System.Serializable]
    public class HexTileDetails
    {
        public string Name;
        public HexSingleTileBifurcation Bifurcations;
        public bool DoubleTile = false;
        public HexDoubleTileBifurcation DoubleTileBifurcations;
        public GameObject HexTile;
    }
    [System.Serializable]
    public class HexSingleTileBifurcation
    {
        public bool Exit_1, Exit_2, Exit_3, Exit_4, Exit_5, End;
    }
    [System.Serializable]
    public class HexDoubleTileBifurcation
    {
        public bool Exit_6, Exit_7, Exit_8, Exit_9, Exit_10,
            Exit_11, Exit_12, Exit_13, Exit_14, Exit_15,
            Exit_16, Exit_17, Exit_18, Exit_19, Exit_20,
            Exit_21, Exit_22;
    }
    [System.Serializable]
    public class PathDetails
    {
        public string ID = "path_";
        public bool m_PathActive = true;
        public HexOrientation PathOrientation = HexOrientation.Zero;
        public Vector3 m_PathNextPosition;
        public GameObject m_Container, m_NextTileChecker;
    }
    public bool CheckPosition(Vector3 position)
    {
        float checkRadius = 5f;
        Collider[] m_Intersecting = Physics.OverlapSphere(position, checkRadius);

        foreach (Collider c in m_Intersecting)
        {
            if (c.CompareTag("Ground"))
            {
                return false; // Hay colisión, no colocar tile
            }
        }

        return true; // No hay colisión, se puede colocar tile
    }
    void ReloadScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentSceneName);
    }
}
