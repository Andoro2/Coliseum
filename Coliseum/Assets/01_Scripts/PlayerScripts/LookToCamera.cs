using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookToCamera : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // Guarda la rotaci�n inicial del canvas.
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // Mantiene la rotaci�n inicial del canvas.
        transform.rotation = initialRotation;
    }
}
