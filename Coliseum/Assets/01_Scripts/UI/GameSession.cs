using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

   
    public PlayerCharacterData m_PlayerCharacterData = new PlayerCharacterData();

    [System.Serializable]
    public class PlayerCharacterData
    {
        public string PJName;
        public Sprite PJIcon;
        public GameObject PJModel;
    }

    public void SetPlayerCharacter(string name, Sprite icon, GameObject model)
    {
        m_PlayerCharacterData.PJName = name;
        m_PlayerCharacterData.PJIcon = icon;
        m_PlayerCharacterData.PJModel = model;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
