using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCameraUI : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 targetPosition = transform.position + mainCamera.transform.rotation * Vector3.forward;
        Vector3 upDirection = mainCamera.transform.rotation * Vector3.up;

        transform.LookAt(targetPosition, upDirection);
    }
}
