using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCharVisual : NetworkBehaviour
{
    [SerializeField] public TextMeshProUGUI pjNameTag;
    [SerializeField] private Button kickButton;

    [SerializeField] private Transform modelParent;

    [SerializeField] private GameObject currentModel;
    private void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            PlayerData playerData = HexGameMultiplayer.Instance.GetPlayerDataFromClientID(OwnerClientId);
            HexGameMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }
    void Start()
    {
        //pjNameTag.text = "";
        //kick button
        if (SceneManager.GetActiveScene().name == "CharacterSelectScene" && gameObject.name != "Player_0") kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
    }
    public void SetPlayerPJ(int PJID)
    {
        pjNameTag.text = HexGameMultiplayer.Instance.GetPJByID(PJID).PJName;

        GameObject prefab = HexGameMultiplayer.Instance.GetPJByID(PJID).PJPreFab;
        if (prefab != null)
            currentModel = Instantiate(prefab, modelParent);
    }
}