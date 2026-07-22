using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class AutoAttack : MonoBehaviour
{
    protected PlayerController PC;
    protected PlayerStats PS;
    public GameObject m_Target;
    public bool m_AutoAim = true;

    [Header("Attack stats")]
    public float m_AttackCooldown = 1f;
    public float m_AttackRange = 5f;
    private float m_LastAttackTime = 0f;

    // BARD
    protected bool m_BardDoubleHit = false;


    protected Animator m_Anim;

    [SerializeField] private Button m_AutoAimButton;
    public event System.Action OnAttack;

    protected virtual void Start()
    {
        PC = GetComponentInParent<PlayerController>();
        PS = GetComponent<PlayerStats>();
        m_Anim = GetComponentInChildren<Animator>();

        m_AutoAimButton = GameObject.FindWithTag("UICanvas").gameObject.transform.GetChild(1).transform.GetChild(0).GetComponent<Button>(); ;
        m_AutoAimButton.onClick.AddListener(() =>
        {
            ToggleAutoAim();
        });
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.IsFighting)
        {
            m_Target = GetNearestEnemy();

            if (m_AutoAim)
                PC.m_AutoAimTarget = m_Target != null ? m_Target.transform : null;
            else
                PC.m_AutoAimTarget = null;

            if (m_Target != null && Time.time >= m_LastAttackTime + GetCooldown())
            {
                if (m_AutoAim) AimAtTarget();

                OnAttack?.Invoke();
                Attack();
                m_LastAttackTime = Time.time;
            }
        }
    }
    protected abstract void Attack();
    private float GetCooldown()
    {
        float cooldown = m_AttackCooldown / (1f + PS.m_AttackSpeedBonusPercent);
        return Mathf.Max(cooldown, 0.4f);
    }

    private GameObject GetNearestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(PC.transform.position, m_AttackRange);
        GameObject nearestEnemy = null;
        float minDist = Mathf.Infinity;

        foreach (Collider enemy in enemiesInRange)
        {
            if (!enemy.CompareTag("Enemy")) continue;

            float dist2Enemy = Vector3.Distance(PC.transform.position, enemy.transform.position);
            if (dist2Enemy < minDist)
            {
                minDist = dist2Enemy;
                nearestEnemy = enemy.gameObject;
            }
        }

        return nearestEnemy;
    }
    private void AimAtTarget()
    {
        if (m_Target == null) return;
        Vector3 lookDir = m_Target.transform.position - PC.transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            PC.transform.rotation = Quaternion.LookRotation(lookDir);
    }
    public void ToggleAutoAim()
    {
        m_AutoAim = !m_AutoAim;
    }
    protected ElementDamage[] BuildElementArray()
    {
        List<ElementDamage> elements = new List<ElementDamage>();
        elements.Add(new ElementDamage { Element = WorldElements.Null, Percentage = 1f });

        foreach (var kvp in PS.m_AutoAttackElements)
            elements.Add(new ElementDamage { Element = kvp.Key, Percentage = kvp.Value });

        return elements.ToArray();
    }

    protected void SpawnAreaDamage(Vector3 position, bool isCrit)
    {
        if (PS == null) return;
        if (PS.m_AreaAttackPrefabs == null || PS.m_AreaAttackPrefabs.Count == 0) return;

        foreach (GameObject areaPrefab in PS.m_AreaAttackPrefabs)
        {
            if (areaPrefab == null) continue;
            GameObject area = Instantiate(areaPrefab, position, Quaternion.identity);
            /*area.GetComponent<HexAreaDamageOnContact>().Initialize(
                PS.m_Damage * PS.m_DamageMultiplier,
                isCrit,
                PS.m_CriticExtra
            );*/
        }

    }

    #region BARD
    public void BardDoubleHitSwitch()
    {
        if (m_BardDoubleHit) m_BardDoubleHit = false;
        else m_BardDoubleHit = true;
    }
    public bool BardDoubleHitCheck()
    {
        bool DoubleHitActive = false;

        if (m_BardDoubleHit) DoubleHitActive = false;
        else DoubleHitActive = true;

        return DoubleHitActive;
    }
    #endregion
}
