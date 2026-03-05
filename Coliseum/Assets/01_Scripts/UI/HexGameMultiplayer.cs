using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HexGameMultiplayer : NetworkBehaviour
{


    private const int PlayerMaxAmount = 6;

    public static HexGameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    //[SerializeField] private List<CharInfo> CharactersInformation = new List<CharInfo>();
    [SerializeField] private List<PJInfo> PJsList = new List<PJInfo>();

    private NetworkList<PlayerData> playerDataNetworkList;


    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }
    
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                // Disconnected!
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }
    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            selectedPJID = 0,
        });
    }
    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != SceneLoader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= PlayerMaxAmount)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromClientID(ulong clientID){
        foreach (PlayerData playerData in playerDataNetworkList){
            if (playerData.clientId == clientID)
            {
                return playerData;
            }
            }
        return default;
    }
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientID(NetworkManager.Singleton.LocalClientId);
    }
    public PlayerData GetPlayerDataFromPlayerIndex (int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public void ChangePlayerPJ(int PJID)
    {
        ChangePlayerPJServerRpc(PJID);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerPJServerRpc(int PJID, ServerRpcParams serverRpcParams = default)
    {
        if (!IsPJAvailable(PJID))
        {
            return;
        }
        // utilitzant un "struct", s'ha d'agafar el que va a menejar, modificar-lo, i després actualitzar-lo
        int playerDataIndex = GetPlayerDataIndexFromClientID(serverRpcParams.Receive.SenderClientId); //grab

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.selectedPJID = PJID;//modify

        playerDataNetworkList[playerDataIndex] = playerData; //update
    }

    public int GetPlayerDataIndexFromClientID(ulong clientID)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientID)
            {
                return i;
            }
        }
        return -1;
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }
    #region PJ Selection
    private bool IsPJAvailable(int PJID)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.selectedPJID == PJID)
            {
                // picked
                return false;
            }
        }
        return true;
    }
    public int GetPlayerPJID(int PJID)
    {
        return PJsList[PJID].PJID;
    }
    public string GetPJName(int PJID)
    {
        return PJsList[PJID].PJName;
    }

    public PJInfo GetPJByID(int idBuscado)
    {
        foreach (PJInfo personaje in PJsList)
        {
            if (personaje.PJID == idBuscado)
            {
                return personaje;
            }
        }
        Debug.LogError($"No se encontró ningún personaje con el ID: {idBuscado}. Devolviendo el primero por defecto.");
        return PJsList[0];
    }
    public bool HasSelectedAPJ()
    {
        return GetPlayerData().selectedPJID != 0;
    }

    [Serializable]
    public class PJInfo
    {
        public string PJName;
        public int PJID;
        public Sprite PJIcon;
        public GameObject PJPreFab;
    }
    #endregion
}
