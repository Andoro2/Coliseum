using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private string m_PJName;
    public Sprite m_PJIcon;
    public GameObject m_PJPrefab,
        m_PJVisual,
        m_VisualModelHolder,
        m_GameData;
    //[SerializeField] private GameObject selectedGameObject;  //per a alguna imatge que marque que es el personatge preseleccionat


    private void Awake()
    {
        m_PJIcon = transform.GetChild(0).GetComponent<Image>().sprite;
        GetComponent<Button>().onClick.AddListener(() =>
        {
            m_GameData.GetComponent<GameSession>().SetPlayerCharacter(m_PJName, m_PJIcon, m_PJPrefab);
            m_VisualModelHolder.GetComponent<CharacterSelectShow>().ShowModel(m_PJVisual);
        });
    }

    /*private void HexGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        throw new System.NotImplementedException();
    }*/
}
