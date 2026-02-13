using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private int PJId;
    [SerializeField] private string PJName;
    //[SerializeField] private GameObject selectedGameObject;  //per a alguna imatge que marque que es el personatge preseleccionat


    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            HexGameMultiplayer.Instance.ChangePlayerPJ(PJId);
        });
    }

    /*private void Start()
    {
        HexGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += HexGameMultiplayer_OnPlayerDataNetworkListChanged;
    }

    private void HexGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        throw new System.NotImplementedException();
    }*/
}
