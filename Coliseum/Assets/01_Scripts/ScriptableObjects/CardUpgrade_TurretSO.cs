using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TurretUpgradeCard", menuName = "ScriptableObjects/TurretUpgradeCard")]
public class CardUpgrade_TurretSO : MonoBehaviour
{
    public string m_Name,
        m_Description;
    public Image m_Icon;
}
