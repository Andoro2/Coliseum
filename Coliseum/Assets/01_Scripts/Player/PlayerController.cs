using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
using static HexPathCreator;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance { get; private set; }
    public enum PlayerStates { Fighting, Building }
    public PlayerStates m_State;
    private GameObject m_FightingUI, m_BuildingUI,
        m_MainCam;

    public float m_Speed;
    private Vector2 m_PlayerMovement,
        m_MouseLook, m_JoystickLook;

    private Vector3 m_RotationTarget; //point where our character will be looking at
    public Transform m_AutoAimTarget;

    public bool isPC;
    
    //dash
    public float dashSpeed = 15f, dashDuration = 0.2f;
    private bool isDashing = false;
    private Vector3 dashDirection;

    //interaction
    private TMP_Text m_InteractionTMP;
    private DetectInteraction DI;

    public Vector3 m_RespawnFromFall = new Vector3(0,10f,0);

    private Animator m_Anim;

    [SerializeField] private List<Vector3> m_SpawanPositionList;
    [SerializeField] private PlayerCharVisual playerVisual;

    public event System.Action OnDashEnd;
    public event System.Action AbilityQUsed;
    public event System.Action AbilityEUsed;
    public event System.Action UltimateUsed;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            GameObject.FindWithTag("GameController").GetComponent<GameManager>().SetLocalPlayerStats(GetComponentInChildren<PlayerStats>());
        }

        if((int)OwnerClientId > 5) transform.position = m_SpawanPositionList[5];

        else transform.position = m_SpawanPositionList[HexGameMultiplayer.Instance.GetPlayerDataIndexFromClientID(OwnerClientId)];

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log(clientId + "has disconnected.");
    }

    private void Awake()
    {        
       //m_MainCam = GameObject.FindWithTag("MainCamera").gameObject;
       //m_MainCam.GetComponent<CameraFollow>().target = transform;
    }
    #region CONTROL
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        m_PlayerMovement = context.ReadValue<Vector2>();
    }
    public void OnMouseLook(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        m_MouseLook = context.ReadValue<Vector2>();
    }
    public void OnJoystickLook(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        m_JoystickLook = context.ReadValue<Vector2>();
    }
    public void OnChangeMode(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

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
        if (!IsOwner) return;

        if (context.performed) StartCoroutine(DashCoroutine());
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (DI.m_Interact && context.performed) Interact();
    }
    public void AbilityQ(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed) UseAbilityQ();
    }
    public void AbilityE(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed) UseAbilityE();
    }
    public void Ultimate(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed) UseUltimate();
    }
    #endregion
    void Start()
    {
        // character visuals
        PlayerData playerData = HexGameMultiplayer.Instance.GetPlayerDataFromClientID(OwnerClientId);
        playerVisual.SetPlayerPJ(playerData.selectedPJID);

        if (IsOwner) m_State = PlayerStates.Fighting;
        else GetComponent<PlayerController>().enabled = false;

        m_MainCam = GameObject.FindWithTag("MainCamera").gameObject;
        m_Anim = playerVisual.currentModel.transform.GetChild(0).transform.GetChild(0).transform.GetComponent<Animator>();

        DI = GetComponentInChildren<DetectInteraction>();

        m_BuildingUI = GameObject.FindWithTag("UICanvas").gameObject.transform.GetChild(1).gameObject;

        m_FightingUI = GameObject.FindWithTag("UICanvas").gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;

        m_InteractionTMP = transform.Find("InteractionCanvas").Find("Text").GetComponent<TMP_Text>();
        m_InteractionTMP.text = "";
    }

    void Update()
    {
        //m_HealthSlider.value = m_CurrentHealth;
        //m_ExpSlider.value = m_CurrentExp;
        //m_HPCurrent.text = m_CurrentHealth.ToString();
        

        #region Mode change
        if (Input.GetKeyDown(KeyCode.Tab) && IsOwner)
        {
            if (m_State == PlayerStates.Building)
            {
                m_MainCam.transform.parent.GetComponent<CameraFollow>().FollowPlayer = true;
                m_MainCam.transform.parent.gameObject.transform.rotation = Quaternion.identity;
                m_State = PlayerStates.Fighting;
            }
            else
            {
                m_MainCam.transform.parent.GetComponent<CameraFollow>().FollowPlayer = false;

                m_State = PlayerStates.Building;
            }
        }
        switch (m_State)
        {
            case PlayerStates.Building:
                //m_FightingUI.SetActive(false);
                m_Anim.SetBool("Building", true);
                m_BuildingUI.SetActive(true);
                break;
            case PlayerStates.Fighting:
                //m_FightingUI.SetActive(true);
                m_Anim.SetBool("Building", false);
                m_BuildingUI.SetActive(false);
                break;
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.X))
        {
            //ObtainExp(10f);
            GetComponentInChildren<PlayerStats>().ObtainExp(50f);
        }

        if (m_MainCam.transform.parent.GetComponent<CameraFollow>().FollowPlayer)
        {
            if (!IsOwner) return;

            if (isPC)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(m_MouseLook);

                if (Physics.Raycast(ray, out hit))
                {
                    m_RotationTarget = hit.point;
                }
            }

            playerMovementAndAim();

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
                transform.position = m_RespawnFromFall;
            }
        }
    }

    public void Interact()
    {

    }
    public void UseAbilityQ()
    {
        AbilityQUsed?.Invoke();
    }
    public void UseAbilityE()
    {
        AbilityEUsed?.Invoke();
    }
    public void UseUltimate()
    {
        UltimateUsed?.Invoke();
    }
    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        m_Anim.SetTrigger("Dash");

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
        OnDashEnd?.Invoke();
    }
    public void playerMovementAndAim()
    {
        Vector3 movement = new Vector3(m_PlayerMovement.x, 0f, m_PlayerMovement.y);

        /*if (movement != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);
            m_Anim.SetBool("Running", true);
        }
        else m_Anim.SetBool("Running", false);

        transform.Translate(movement * m_Speed * Time.deltaTime, Space.World);*/

        Vector3 lookDir = Vector3.zero;

        if (m_AutoAimTarget != null) // enemigo en rango + autoaim activo
        {
            lookDir = m_AutoAimTarget.position - transform.position;
        }
        else if (isPC) // PC sin autoaim o sin enemigo en rango
        {
            lookDir = m_RotationTarget - transform.position;
        }
        else // mando
        {
            if (m_JoystickLook.x != 0 || m_JoystickLook.y != 0)
                lookDir = new Vector3(m_JoystickLook.x, 0f, m_JoystickLook.y);
            else if (movement != Vector3.zero)
                lookDir = movement;
        }

        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 0.15f);

        // --- MOVIMIENTO ---
        transform.Translate(movement * m_Speed * Time.deltaTime, Space.World);

        if (movement != Vector3.zero) m_Anim.SetBool("Running", true);
        else m_Anim.SetBool("Running", false);
    }
    /*public void movePlayerWithAim()
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

        if (movement != Vector3.zero) m_Anim.SetBool("Running", true);
        else m_Anim.SetBool("Running", false);
    }
    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Respawn"))
        {
            m_RespawnFromFall = other.transform;
        }
    }*/

    #region Level managing
    /*
    [System.Serializable]
    public class LevelAttributes
    {
        public int m_Level;
        public float m_MaxHealth, m_ExpToAdvance;
    }
    public void ObtainExp(float exp)
    {
        m_CurrentExp += exp;

        if(m_CurrentExp >= m_LevelsArray[m_Level].m_ExpToAdvance && m_Level + 1 < m_LevelsArray.Count)
        {
            m_Level++;

            m_HealthSlider.maxValue = m_LevelsArray[m_Level].m_MaxHealth;
            m_HealthSlider.value = m_LevelsArray[m_Level].m_MaxHealth;

            m_MaxHealth = m_LevelsArray[m_Level].m_MaxHealth;
            m_CurrentHealth = m_LevelsArray[m_Level].m_MaxHealth;

            m_ExpSlider.minValue = m_LevelsArray[m_Level - 1].m_ExpToAdvance;
            m_ExpSlider.maxValue = m_LevelsArray[m_Level].m_ExpToAdvance;

            m_HPMax.text = "/" + m_LevelsArray[m_Level].m_MaxHealth;
        }
    }*/
    #endregion
}
