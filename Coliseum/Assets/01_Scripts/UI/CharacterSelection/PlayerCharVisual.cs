using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCharVisual : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI pjNameTag;
    // Start is called before the first frame update
    void Start()
    {
        pjNameTag.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerPJ(int PJID)
    {
        pjNameTag.text = HexGameMultiplayer.Instance.GetPJByID(PJID).PJName;
    }
}
