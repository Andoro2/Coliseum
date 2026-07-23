using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private string PJName;
    public Sprite PJIcon;
    [SerializeField] GameObject PJModel;
    //[SerializeField] private GameObject selectedGameObject;  //per a alguna imatge que marque que es el personatge preseleccionat


    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            //HexGameMultiplayer.Instance.ChangePlayerPJ(PJId);
        });
        PJIcon = GetComponent<Image>().sprite;
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
