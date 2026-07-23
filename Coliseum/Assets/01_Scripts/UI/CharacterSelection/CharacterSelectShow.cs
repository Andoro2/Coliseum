using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectShow : MonoBehaviour
{
    public GameObject m_VisualModel;
    public void ShowModel(GameObject visual)
    {
        if(m_VisualModel != null)
        {
            m_VisualModel.SetActive(false);
        }
        m_VisualModel = visual;
        m_VisualModel.SetActive(true);
    }
}
