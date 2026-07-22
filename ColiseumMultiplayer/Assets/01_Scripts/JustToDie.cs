using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustToDie : MonoBehaviour
{
    public void Die()
    {
        Destroy(transform.root.gameObject);
    }
}
