using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class ConnectionResponseUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButon;

    private void Awake()
    {
        closeButon.onClick.AddListener(Hide);
    }
    private void Start()
    {
        HexGameMultiplayer.Instance.OnFailedToJoinGame += HexGameMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void HexGameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if (messageText.text == "")
        {
            messageText.text = "Failed to connect";
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        HexGameMultiplayer.Instance.OnFailedToJoinGame -= HexGameMultiplayer_OnFailedToJoinGame;
    }
}
