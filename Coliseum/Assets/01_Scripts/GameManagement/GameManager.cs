using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int m_Currency = 500, m_Wave = 1;
    public bool IsFighting;
    public TextMeshProUGUI m_WavelTMP, m_CurrencyTMP;
    private List<GameObject> tileCanvases = new List<GameObject>();
    void Start()
    {
        m_WavelTMP.text = "" + m_Wave;
        m_CurrencyTMP.text = "" + m_Currency;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break(); // Pauses the editor if in Play Mode
        }


        if (int.Parse(m_WavelTMP.text) != m_Wave)
        {
            m_WavelTMP.text = "" + m_Wave;
        }
        if (int.Parse(m_CurrencyTMP.text) != m_Currency)
        {
            m_CurrencyTMP.text = "" + m_Currency;
        }
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
}
