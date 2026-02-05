using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        HexGameMultiplayer.Instance.OnTryingToJoinGame += HexGameMultiplayer_OnTryingToJoinGame;
        HexGameMultiplayer.Instance.OnFailedToJoinGame += HexGameMultiplayer_OnFailedToJoinGame;
        Hide();
    }
    private void HexGameMultiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
    }
    private void HexGameMultiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
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
        HexGameMultiplayer.Instance.OnTryingToJoinGame -= HexGameMultiplayer_OnTryingToJoinGame;
        HexGameMultiplayer.Instance.OnFailedToJoinGame -= HexGameMultiplayer_OnFailedToJoinGame;
    }
}
