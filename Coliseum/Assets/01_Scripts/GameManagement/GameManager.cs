using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
// using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    //public enum GameStates { Fighting, Building }
    //public GameStates m_State = GameStates.Fighting;
    //public GameObject m_FightingUI, m_BuildingUI;
    [Header("Attack stuff:")]
    public bool IsFighting;

    private GameObject m_MainCam;

    public int m_TowerMaxLife = 20, m_TowerCurrentLife, m_Currency = 500, m_Wave = 1;
    public TextMeshProUGUI m_HealthTMP, m_WaveTMP, m_CurrencyTMP;
    private List<GameObject> tileCanvases = new List<GameObject>();
    [HideInInspector] public bool currencyFlag = false;

    [SerializeField] private Transform playerPrefab;

    private Dictionary<ClassEnum, bool> m_PresentClasses = new Dictionary<ClassEnum, bool>();
    public enum ClassEnum
    {
        Artificer, Barbarian, Bard, Cleric, Druid, Fighter, Monk, Paladin, Ranger, Rogue, Warlock, Wizard,
    }
    private PlayerStats PS;

    public void SetClassPresent(ClassEnum m_Class)
    { 
        m_PresentClasses[m_Class] = true;
    }

    void Start()
    {
        m_WaveTMP.text = "" + m_Wave;
        m_CurrencyTMP.text = "" + m_Currency;

        m_MainCam = GameObject.FindWithTag("MainCamera");

        PS = PlayerController.LocalInstance.GetComponentInChildren<PlayerStats>();

        m_TowerCurrentLife = m_TowerMaxLife;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break(); // Pauses the editor if in Play Mode
        }

        if (PS != null)
        {
            if (int.Parse(m_HealthTMP.text) != PS.m_CurrentHealth.Value)
            {
                m_HealthTMP.text = "" + PS.m_CurrentHealth;
            }
            if (int.Parse(m_WaveTMP.text) != m_Wave)
            {
                m_WaveTMP.text = "" + m_Wave;
            }
            if (int.Parse(m_CurrencyTMP.text) != m_Currency)
            {
                m_CurrencyTMP.text = "" + m_Currency;
            }
        }
        
        /*#region Mode change
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(m_State== GameStates.Building)
            {
                GameObject.FindWithTag("MainCamera").gameObject.transform.parent.GetComponent<CameraFollow>().FollowPlayer = true;
                GameObject.FindWithTag("MainCamera").gameObject.transform.parent.gameObject.transform.rotation = Quaternion.identity;
                m_State = GameStates.Fighting;
            }
            else
            {
                GameObject.FindWithTag("MainCamera").gameObject.transform.parent.GetComponent<CameraFollow>().FollowPlayer = false;

                m_State = GameStates.Building;
            }
        }
        switch (m_State)
        {
            case GameStates.Building:
                //m_FightingUI.SetActive(false);
                m_BuildingUI.SetActive(true);
                break;
            case GameStates.Fighting:
                //m_FightingUI.SetActive(true);
                m_BuildingUI.SetActive(false);
                break;
        }
        #endregion*/

        #region Tile cretion buttons
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("CreateTileCanvas");

        foreach (GameObject obj in foundObjects)
        {
            if (!tileCanvases.Contains(obj))
            {
                tileCanvases.Add(obj);
            }
        }

        foreach (GameObject obj in tileCanvases)
        {
            if (obj == null) continue;

            if (IsFighting && obj.activeSelf)
            {
                obj.SetActive(false);
            }
            else if (!IsFighting && !obj.activeSelf)
            {
                obj.SetActive(true);
            }
        }
        #endregion
    }
    public void DamageTower(int dmg)
    {
        m_TowerCurrentLife -= dmg;

        //if (m_TowerCurrentLife <= 0) EndGame();
    }
    public void HealTower(int heal)
    {
        if ((m_TowerCurrentLife + heal) > m_TowerMaxLife) m_TowerCurrentLife = m_TowerMaxLife;
        else m_TowerCurrentLife += heal;
    }
    public void GetPaid(int money)
    {
        m_Currency += money;
    }
    public void SpendMoney(int money)
    {
        if(m_PresentClasses[ClassEnum.Artificer] && currencyFlag) m_Currency -= Mathf.FloorToInt((money * 0.9f));
        else m_Currency -= money;
    }
    public void NextWave()
    {
        m_Wave++;
    }

    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        }
    }
    public void SetLocalPlayerStats(PlayerStats ps)
    {
        PS = ps;
    }
}
