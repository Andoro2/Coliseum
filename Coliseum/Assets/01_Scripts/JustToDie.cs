using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustToDie : MonoBehaviour
{
    public GameObject m_ToDestroy;
    public void Die()
    {
        Destroy(m_ToDestroy);
    }
}
