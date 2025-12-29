using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerUpgradeCard", menuName = "ScriptableObjects/PlayerUpgradeCard")]
public class CardUpgrade_PlayerSO : MonoBehaviour
{
    public string m_Name,
        m_Description;
    public Image m_Icon;
}
