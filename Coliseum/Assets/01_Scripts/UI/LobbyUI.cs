using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : NetworkBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {

        createGameButton.onClick.AddListener(() =>
        {
            HexGameMultiplayer.Instance.StartHost();
            SceneLoader.LoadNetwork(SceneLoader.Scene.CharacterSelectScene);
        });

        joinGameButton.onClick.AddListener(() =>
        {
            HexGameMultiplayer.Instance.StartClient();
        });
    }

    
}
