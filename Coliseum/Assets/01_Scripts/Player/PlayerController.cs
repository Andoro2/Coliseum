using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    public float m_Speed;
    private Vector2 m_PlayerMovement,
        m_MouseLook, m_JoystickLook;

    private Vector3 m_RotationTarget; //point where our character will be looking at

    public bool isPC;
    
    //dash
    public float dashSpeed = 15f, dashDuration = 0.2f;
    private bool isDashing = false;
    private Vector3 dashDirection;

    //interaction
    private TMP_Text m_InteractionTMP;
    private DetectInteraction DI;

    public Transform m_RespawnFromFall;

    public void OnMove(InputAction.CallbackContext context)
    {
        m_PlayerMovement = context.ReadValue<Vector2>();
    }
    public void OnMouseLook(InputAction.CallbackContext context)
    {
        m_MouseLook = context.ReadValue<Vector2>();
    }
    public void OnJoystickLook(InputAction.CallbackContext context)
    {
        m_JoystickLook = context.ReadValue<Vector2>();
    }
    public void OnChangeMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            bool changeMode = context.ReadValueAsButton();
            if (changeMode)
            {
                if (isPC) isPC = false;
                else isPC = true;
            }
        }
    }
    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed) StartCoroutine(DashCoroutine());
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (DI.m_Interact && context.performed) Interact();
    }
    void Start()
    {
        DI = GetComponentInChildren<DetectInteraction>();

        m_InteractionTMP = transform.Find("InteractionCanvas").Find("Text").GetComponent<TMP_Text>();
        m_InteractionTMP.text = "";
    }

    void Update()
    {
        if (isPC)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(m_MouseLook);

            if(Physics.Raycast(ray, out hit))
            {
                m_RotationTarget = hit.point;
            }

            movePlayerWithAim();
        }
        else
        {
            if(m_JoystickLook.x == 0 && m_JoystickLook.y == 0)
            {
                playerMovement();
            }
            else
            {
                movePlayerWithAim();
            }
        }

        if (DI.m_Interact)
        {
            m_InteractionTMP.text = DI.m_InteractionType;
        }
        else
        {
            m_InteractionTMP.text = "";
        }

        if (transform.position.y < -5f)
        {
            transform.position = m_RespawnFromFall.position;
        }
    }

    public void Interact()
    {

    }
    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        if (m_PlayerMovement != Vector2.zero)
        {
            dashDirection = new Vector3(m_PlayerMovement.x, 0f, m_PlayerMovement.y).normalized;
        }
        else
        {
            if (isPC)
            {
                dashDirection = (m_RotationTarget - transform.position).normalized;
                dashDirection.y = 0;
            }
            else
            {
                if (m_JoystickLook == Vector2.zero)
                {
                    dashDirection = transform.forward;
                }
                else
                {
                    dashDirection = new Vector3(m_JoystickLook.x, 0f, m_JoystickLook.y).normalized;
                }
            }
        }

        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            transform.Translate(dashDirection * dashSpeed * Time.deltaTime, Space.World);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
    public void playerMovement()
    {
        Vector3 movement = new Vector3(m_PlayerMovement.x, 0f, m_PlayerMovement.y);

        if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);

        }

        transform.Translate(movement * m_Speed * Time.deltaTime, Space.World);
    }
    public void movePlayerWithAim()
    {
        if (isPC)
        {
            var lookPos = m_RotationTarget - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);

            Vector3 aimDir = new Vector3(m_RotationTarget.x, 0f, m_RotationTarget.y);

            if(aimDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
            }
        }
        else
        {
            Vector3 aimDir= new Vector3(m_JoystickLook.x, 0f, m_JoystickLook.y);

            if (aimDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(aimDir), 0.15f);
            }
        }

        Vector3 movement = new Vector3(m_PlayerMovement.x, 0f, m_PlayerMovement.y);

        transform.Translate(movement * m_Speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            m_RespawnFromFall = other.transform;
        }
    }
}
