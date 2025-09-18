using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    //public enum GameStates { Fighting, Building }
    //public GameStates m_State = GameStates.Fighting;
    //public GameObject m_FightingUI, m_BuildingUI;

    private GameObject m_MainCam;

    public int m_BaseHealth = 20, m_Currency = 500, m_Wave = 1;
    public bool IsFighting;
    public TextMeshProUGUI m_HealthTMP, m_WavelTMP, m_CurrencyTMP;
    private List<GameObject> tileCanvases = new List<GameObject>();
    void Start()
    {
        m_WavelTMP.text = "" + m_Wave;
        m_CurrencyTMP.text = "" + m_Currency;

        m_MainCam = GameObject.FindWithTag("MainCamera");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break(); // Pauses the editor if in Play Mode
        }

        if (int.Parse(m_HealthTMP.text) != m_BaseHealth)
        {
            m_HealthTMP.text = "" + m_BaseHealth;
        }
        if (int.Parse(m_WavelTMP.text) != m_Wave)
        {
            m_WavelTMP.text = "" + m_Wave;
        }
        if (int.Parse(m_CurrencyTMP.text) != m_Currency)
        {
            m_CurrencyTMP.text = "" + m_Currency;
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
    public void TakeDamage(int dmg)
    {
        m_BaseHealth -= dmg;
    }
    public void HealDamage(int heal)
    {
        m_BaseHealth += heal;
    }
    public void GetPaid(int money)
    {
        m_Currency += money;
    }
    public void SpendMoney(int money)
    {
        m_Currency -= money;
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
}
