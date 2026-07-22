using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectInteraction : MonoBehaviour
{
    public bool m_Interact = false;
    public string m_InteractionType;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interact"))
        {
            m_InteractionType = other.gameObject.GetComponent<InteractionType>().m_InteractionType;
            m_Interact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interact"))
        {
            m_Interact = false;
        }
    }
}
