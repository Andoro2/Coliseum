using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public class PathCreator : MonoBehaviour
{
    //public int m_PathNumber = 1;
    public enum Orientation { Up, Down, Left, Right, End }
    [SerializeField] private static Orientation PathOrientation = Orientation.Up;
    public enum PathTypes { Straight, TurnLeft, TurnRight, End }
    [SerializeField] private static PathTypes PathType = PathTypes.End;
    public Vector3 m_LastTilePosition = new Vector3(0, -1.55f, 55);
    public List<TileDetails> m_Paths = new List<TileDetails>();

    public GameObject m_TileFather;
    public string orientacion = "up";

    public EnemySpawner m_EnemySpawner;
    void Start()
    {
        m_EnemySpawner = GetComponent<EnemySpawner>();

        List<TileDetails> InitialPaths = new List<TileDetails>();
        foreach (TileDetails Tile in m_Paths)
        {
            if(Tile.PathType == PathTypes.Straight)
            {
                InitialPaths.Add(Tile);
            }
        }
        CreateTile(InitialPaths, m_LastTilePosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
        if (Input.GetKeyDown(KeyCode.Space) && PathOrientation != Orientation.End)
        {
            MakePath(m_LastTilePosition);
        }

        switch (PathOrientation)
        {
            case Orientation.Up:
                orientacion = "up";
                break;
            case Orientation.Down:
                orientacion = "down";
                break;
            case Orientation.Left:
                orientacion = "left";
                break;
            case Orientation.Right:
                orientacion = "right";
                break;
            case Orientation.End:
                orientacion = "end";
                break;
        }
    }
    void ReloadScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentSceneName);
    }

    [System.Serializable]
    public class TileDetails
    {
        public string Name;
        public PathTypes PathType = PathTypes.End;
        public GameObject Tile;
    }
    public void CreateTile(List<TileDetails> m_AvailablePaths, Vector3 NextTilePos)
    {
        if (m_AvailablePaths.Count == 0)
        {
            Debug.Log("No hay caminos disponibles, fin del camino.");
            return;
        }

        TileDetails Tile = m_AvailablePaths[Random.Range(0, m_AvailablePaths.Count)];
        GameObject GeneratedTile = Instantiate(Tile.Tile, NextTilePos, Quaternion.identity);

        float yRotation = 0;

        switch (PathOrientation)
        {
            case Orientation.Up:
                yRotation = 0;
                break;
            case Orientation.Down:
                yRotation = 180;
                break;
            case Orientation.Left:
                yRotation = 270;
                break;
            case Orientation.Right:
                yRotation = 90;
                break;
        }

        yRotation = (yRotation + 360) % 360;
        GeneratedTile.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        if (Tile.PathType == PathTypes.TurnLeft)
        {
            PathOrientation = TurnLeft(PathOrientation);
        }
        else if (Tile.PathType == PathTypes.TurnRight)
        {
            PathOrientation = TurnRight(PathOrientation);
        }
        else if (Tile.PathType == PathTypes.End)
        {
            PathOrientation = Orientation.End;
        }

        GeneratedTile.transform.SetParent(m_TileFather.transform);
        m_LastTilePosition = NextTilePos;

        m_EnemySpawner.SetNewSpawnPoint(GeneratedTile.transform.GetChild(2).transform.gameObject);
    }


    private Orientation TurnLeft(Orientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case Orientation.Up: return Orientation.Left;
            case Orientation.Down: return Orientation.Right;
            case Orientation.Left: return Orientation.Down;
            case Orientation.Right: return Orientation.Up;
            default: return currentOrientation;
        }
    }

    private Orientation TurnRight(Orientation currentOrientation)
    {
        switch (currentOrientation)
        {
            case Orientation.Up: return Orientation.Right;
            case Orientation.Down: return Orientation.Left;
            case Orientation.Left: return Orientation.Up;
            case Orientation.Right: return Orientation.Down;
            default: return currentOrientation;
        }
    }

    public void MakePath(Vector3 TilePosition)
    {
        List<TileDetails> m_AvailablePaths = new List<TileDetails>();

        Vector3 nextTilePos = TilePosition;
        //Vector3 nextTStraight, nextTLeft, nextTRight;

        switch (PathOrientation) // set position for the next tile
        {
            case Orientation.Up:
                nextTilePos.z += 35; break;
            case Orientation.Down:
                nextTilePos.z -= 35; break;
            case Orientation.Left:
                nextTilePos.x -= 35; break;
            case Orientation.Right:
                nextTilePos.x += 35; break;
        }

        // check straight direction
        Vector3 CheckStraight(Vector3 nextTilePos, Orientation direction)
        {
            Vector3 checkThisPos = nextTilePos;
            switch (PathOrientation)
            {
                case Orientation.Up:
                    checkThisPos.z += 35;
                    break;
                case Orientation.Down:
                    checkThisPos.z -= 35;
                    break;
                case Orientation.Left:
                    checkThisPos.x -= 35;
                    break;
                case Orientation.Right:
                    checkThisPos.x += 35;
                    break;
            }
            if (CheckPosition(checkThisPos))
            {
                return checkThisPos;
            }
            else
            {
                return Vector3.zero;
            }
        }

        Vector3 CheckRight(Vector3 nextTilePos, Orientation direction)
        {
            Vector3 checkThisPos = nextTilePos;
            switch (direction)
            {
                case Orientation.Up:
                    checkThisPos = new Vector3(nextTilePos.x + 35, nextTilePos.y, nextTilePos.z);
                    break;
                case Orientation.Down:
                    checkThisPos = new Vector3(nextTilePos.x - 35, nextTilePos.y, nextTilePos.z);
                    break;
                case Orientation.Left:
                    checkThisPos = new Vector3(nextTilePos.x, nextTilePos.y, nextTilePos.z + 35);
                    break;
                case Orientation.Right:
                    checkThisPos = new Vector3(nextTilePos.x, nextTilePos.y, nextTilePos.z - 35);
                    break;
            }
            if (CheckPosition(checkThisPos))
            {
                return checkThisPos;
            }
            else
            {
                return Vector3.zero;
            }
        }

        Vector3 CheckLeft(Vector3 nextTilePos, Orientation direction)
        {
            Vector3 checkThisPos = nextTilePos;
            switch (direction)
            {
                case Orientation.Up:
                    checkThisPos = new Vector3(nextTilePos.x - 35, nextTilePos.y, nextTilePos.z);
                    break;
                case Orientation.Down:
                    checkThisPos = new Vector3(nextTilePos.x + 35, nextTilePos.y, nextTilePos.z);
                    break;
                case Orientation.Left:
                    checkThisPos = new Vector3(nextTilePos.x, nextTilePos.y, nextTilePos.z - 35);
                    break;
                case Orientation.Right:
                    checkThisPos = new Vector3(nextTilePos.x, nextTilePos.y, nextTilePos.z + 35);
                    break;
            }
            if (CheckPosition(checkThisPos))
            {
                return checkThisPos;
            }
            else
            {
                return Vector3.zero;
            }
        }

        bool hasOtherOptions = false;

        if (CheckStraight(nextTilePos, PathOrientation) != Vector3.zero)
        {
            m_AvailablePaths.AddRange(m_Paths.Where(p => p.PathType == PathTypes.Straight));
            hasOtherOptions = true;
        }
        if (CheckRight(nextTilePos, PathOrientation) != Vector3.zero)
        {
            m_AvailablePaths.AddRange(m_Paths.Where(p => p.PathType == PathTypes.TurnRight));
            hasOtherOptions = true;
        }
        if (CheckLeft(nextTilePos, PathOrientation) != Vector3.zero)
        {
            m_AvailablePaths.AddRange(m_Paths.Where(p => p.PathType == PathTypes.TurnLeft));
            hasOtherOptions = true;
        }

        if (!hasOtherOptions)
        {
            m_AvailablePaths.AddRange(m_Paths.Where(p => p.PathType == PathTypes.End));
        }

        CreateTile(m_AvailablePaths, nextTilePos);
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
}