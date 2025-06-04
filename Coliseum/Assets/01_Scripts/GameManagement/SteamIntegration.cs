using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamIntegration : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Steamworks.SteamClient.Init(3703620);//game id
            PrintYourName();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
    private void PrintYourName()
    {
        Debug.Log(Steamworks.SteamClient.Name);
    }
    void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }
    void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }
}
