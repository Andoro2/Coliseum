using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    public GameSession gameSession;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
        readyButton.onClick.AddListener(() =>
        {
            //if(HexGameMultiplayer.Instance.HasSelectedAPJ()) CharacterSelectReady.Instance.SetPlayerReady();
            if (gameSession.m_PlayerCharacterData != null) SceneManager.LoadScene(2);
        });
    }
}
