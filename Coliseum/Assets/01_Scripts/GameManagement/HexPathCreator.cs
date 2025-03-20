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
        InitialTiles.AddRange(m_HexTiles.Where(p => !p.Bifurcations.ExitOne && p.Bifurcations.ExitTwo && !p.Bifurcations.ExitThree && !p.Bifurcations.ExitFour && !p.Bifurcations.ExitFive && !p.Bifurcations.End));
        InitialTiles.AddRange(m_HexTiles.Where(p => !p.Bifurcations.ExitOne && !p.Bifurcations.ExitTwo && p.Bifurcations.ExitThree && !p.Bifurcations.ExitFour && !p.Bifurcations.ExitFive && !p.Bifurcations.End));
        InitialTiles.AddRange(m_HexTiles.Where(p => !p.Bifurcations.ExitOne && !p.Bifurcations.ExitTwo && !p.Bifurcations.ExitThree && p.Bifurcations.ExitFour && !p.Bifurcations.ExitFive && !p.Bifurcations.End));

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

                //path.m_PathNextPosition += UpdateNextTilePos(path);


                int yRotation = (adjustRotation(path) + 360) % 360;

                m_NextTileChecker.transform.rotation = Quaternion.Euler(0, yRotation, 0);


                GameObject PathContainer = Instantiate(m_PathHolder, path.m_PathNextPosition, Quaternion.identity);
                path.m_Container = PathContainer;

                path.m_NextTileChecker = Instantiate(m_NextTileChecker, m_PathStartPositions[i], Quaternion.identity);
                path.m_NextTileChecker.name = "TileChecker_" + path.ID;

                PathContainer.name = path.ID;

                m_PathList.Add(path);

                BuildTrack(path, InitialTiles, m_PathStartPositions[i]);
            }
        }
        InvokeRepeating("AutoGenerate", 0f, 0.1f);
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
            case 1: path.PathOrientation = CloseTurnLeft(PrevPath.PathOrientation); break;
            case 2: path.PathOrientation = LongTurnLeft(PrevPath.PathOrientation); break;
            case 3: path.PathOrientation = PrevPath.PathOrientation; break;
            case 4: path.PathOrientation = LongTurnRight(PrevPath.PathOrientation); break;
            case 5: path.PathOrientation = CloseTurnRight(PrevPath.PathOrientation); break;
        }
        path.m_NextTileChecker = Instantiate(PrevPath.m_NextTileChecker, GeneratedTile.transform.position + UpdateNextTilePos(path), Quaternion.identity);

        float yRotation = (adjustRotation(path) + 360) % 360;
        path.m_NextTileChecker.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        path.m_NextTileChecker.name = "TileChecker_" + path.ID;

        path.m_PathNextPosition += UpdateNextTilePos(path);
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

        if (CheckPosition(Track.m_NextTileChecker.transform.Find("ExitOne").position))
            m_AvailableExits.Add(1);
        if (CheckPosition(Track.m_NextTileChecker.transform.Find("ExitTwo").position))
            m_AvailableExits.Add(2);
        if (CheckPosition(Track.m_NextTileChecker.transform.Find("ExitThree").position))
            m_AvailableExits.Add(3);
        if (CheckPosition(Track.m_NextTileChecker.transform.Find("ExitFour").position))
            m_AvailableExits.Add(4);
        if (CheckPosition(Track.m_NextTileChecker.transform.Find("ExitFive").position))
            m_AvailableExits.Add(5);



        foreach (HexTileDetails tile in m_HexTiles)
        {
            HashSet<int> tileExits = new HashSet<int>();

            if (tile.Bifurcations.ExitOne) tileExits.Add(1);
            if (tile.Bifurcations.ExitTwo) tileExits.Add(2);
            if (tile.Bifurcations.ExitThree) tileExits.Add(3);
            if (tile.Bifurcations.ExitFour) tileExits.Add(4);
            if (tile.Bifurcations.ExitFive) tileExits.Add(5);

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
        GameObject GeneratedTile = Instantiate(Tile.HexTile, NextTilePos, Quaternion.identity);

        float yRotation = 0;
        
        yRotation = (adjustRotation(Path) + 360) % 360;
        GeneratedTile.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        int bifurcation = 0;
        if (CheckDivisions(Tile) > 1)
        {
            int lastBifurcation = 0;
            if (Tile.Bifurcations.ExitOne)
            {
                bifurcation = 1;
                if (lastBifurcation == 0)
                {
                    lastBifurcation = bifurcation;
                }
                else PathBifurcation(Path, bifurcation, GeneratedTile);
            }
            if (Tile.Bifurcations.ExitTwo)
            {
                bifurcation = 2;
                if (lastBifurcation == 0)
                {
                    lastBifurcation = bifurcation;
                }
                else PathBifurcation(Path, bifurcation, GeneratedTile);
            }
            if (Tile.Bifurcations.ExitThree)
            {
                bifurcation = 3;
                if (lastBifurcation == 0)
                {
                    lastBifurcation = bifurcation;
                }
                else PathBifurcation(Path, bifurcation, GeneratedTile);
            }
            if (Tile.Bifurcations.ExitFour)
            {
                bifurcation = 4;
                if (lastBifurcation == 0)
                {
                    lastBifurcation = bifurcation;
                }
                else PathBifurcation(Path, bifurcation, GeneratedTile);
            }
            if (Tile.Bifurcations.ExitFive)
            {
                bifurcation = 5;
                if (lastBifurcation == 0)
                {
                    lastBifurcation = bifurcation;
                }
                else PathBifurcation(Path, bifurcation, GeneratedTile);
            }

            if (lastBifurcation != 0)
            {
                UpdatePath(lastBifurcation, Path, GeneratedTile);
            }
        }
        else
        {
            if (Tile.Bifurcations.ExitOne) bifurcation = 1;
            if (Tile.Bifurcations.ExitTwo) bifurcation = 2;
            if (Tile.Bifurcations.ExitThree) bifurcation = 3;
            if (Tile.Bifurcations.ExitFour) bifurcation = 4;
            if (Tile.Bifurcations.ExitFive) bifurcation = 5;
            if (Tile.Bifurcations.End) bifurcation = 0;

            UpdatePath(bifurcation, Path, GeneratedTile);
        }
    }
    private void UpdatePath(int HexTileBifurcation, PathDetails Path, GameObject GeneratedTile)
    {
        if (HexTileBifurcation == 1) Path.PathOrientation = CloseTurnLeft(Path.PathOrientation);
        if (HexTileBifurcation == 2) Path.PathOrientation = LongTurnLeft(Path.PathOrientation);
        if (HexTileBifurcation == 4) Path.PathOrientation = LongTurnRight(Path.PathOrientation);
        if (HexTileBifurcation == 5) Path.PathOrientation = CloseTurnRight(Path.PathOrientation);
        if (HexTileBifurcation == 0)
        {
            Path.PathOrientation = HexOrientation.End;
            GeneratedTile.gameObject.transform.SetParent(Path.m_Container.gameObject.transform);
            Destroy(Path.m_NextTileChecker);
            m_PathList.Remove(Path);
            return;
        }

        Path.m_PathNextPosition += UpdateNextTilePos(Path);

        Path.m_NextTileChecker.transform.position = Path.m_PathNextPosition;

        float yRotation = (adjustRotation(Path) + 360) % 360;

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

        if (Tile.Bifurcations.ExitOne) exitCount++;
        if (Tile.Bifurcations.ExitTwo) exitCount++;
        if (Tile.Bifurcations.ExitThree) exitCount++;
        if (Tile.Bifurcations.ExitFour) exitCount++;
        if (Tile.Bifurcations.ExitFive) exitCount++;

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
    private Vector3 UpdateNextTilePos(PathDetails PathOrientation)
    {
        Vector3 NewPos = new Vector3();
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
        return NewPos;
    }
    [System.Serializable]
    public class HexTileDetails
    {
        public string Name;
        public HexTileBifurcation Bifurcations;
        public bool DoubleTile = false;
        public GameObject HexTile;
    }
    [System.Serializable]
    public class HexTileBifurcation
    {
        public bool ExitOne, ExitTwo, ExitThree, ExitFour, ExitFive, End;
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
