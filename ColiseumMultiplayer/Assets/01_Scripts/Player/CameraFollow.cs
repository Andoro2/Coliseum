using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static TileElementAsigned;

public class CameraFollow : NetworkBehaviour
{
    public bool FollowPlayer = false;
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        target = null;
        transform.position = Vector3.zero;
    }
    //[ClientRpc]
    public void AssignTarget()//ClientRpc()
    {
        //target = GameObject.FindObjectWithTag("Player").gameObject.transform;

        //List<Object> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            NetworkObject netObj = p.GetComponent<NetworkObject>();

            if (netObj != null && netObj.IsOwner)
            {
                target = p.transform;
            }
        }
        
    }
    void Update()
    {
        if (target == null) AssignTarget();// ClientRpc(); 

        if (target != null && FollowPlayer)
            {
            Vector3 targetPos = target.position + offset;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
        else
        {
            Movement();
        }
    }
    public float moveSpeed = 25f;
    public float rotationSpeed = 100f;
    void Movement()
    {
        // Movimiento vertical (W/S)
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }

        // Movimiento lateral (A/D)
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // Rotación con Q y E
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
