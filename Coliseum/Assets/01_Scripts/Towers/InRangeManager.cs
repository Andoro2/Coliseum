using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class InRangeManager : NetworkBehaviour
{
    public enum TargetPriority
    {
        FirstInList,
        MostHealth,
        LeastHealth,
        Slowest,
        Fastest,
        WeakToElement,
    }

    // Longitud de un lado del hexágono. Es el único valor que hay que ajustar para cambiar el rango.
    public float hexSize = 3f;

    // Altura del área de detección
    public float detectionHeight = 2f;

    // Derivados internos
    private float HexWidth  => hexSize * Mathf.Sqrt(3f) * 2f;
    private float HexHeight => hexSize * 2f;

    public float checkInterval = 0.1f;
    public LayerMask enemyLayer;

    // Prioridad elegida por el jugador desde la UI
    private NetworkVariable<TargetPriority> targetPriority = new NetworkVariable<TargetPriority>(
        TargetPriority.FirstInList,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Elementos configurados por el diseñador desde el Inspector
    public List<WorldElements> targetElements = new List<WorldElements>();

    public HashSet<GameObject> enemiesInRange = new HashSet<GameObject>();

    // Lista auxiliar solo para visualización en el Inspector durante el juego en editor
    [SerializeField] private List<GameObject> m_EnemiesInRangeDebug = new List<GameObject>();

    private static readonly Quaternion[] boxRotations = new Quaternion[]
    {
        Quaternion.Euler(0,   0, 0),
        Quaternion.Euler(0,  60, 0),
        Quaternion.Euler(0, 120, 0)
    };

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            InvokeRepeating(nameof(UpdateEnemiesInRange), 0f, checkInterval);
    }

    private void UpdateEnemiesInRange()
    {
        enemiesInRange.Clear();

        Vector3 halfExtents = new Vector3(HexWidth / 2f, detectionHeight / 2f, HexHeight / 2f);

        foreach (Quaternion rotation in boxRotations)
        {
            Collider[] hits = Physics.OverlapBox(
                transform.position,
                halfExtents,
                rotation,
                enemyLayer
            );

            foreach (Collider col in hits)
            {
                if (col.CompareTag("Enemy"))
                    enemiesInRange.Add(col.gameObject);
            }
        }

#if UNITY_EDITOR
        m_EnemiesInRangeDebug = new List<GameObject>(enemiesInRange);
#endif
    }

    public GameObject GetPriorityTarget()
    {
        enemiesInRange.RemoveWhere(e => e == null);

        if (enemiesInRange.Count == 0) return null;

        switch (targetPriority.Value)
        {
            case TargetPriority.FirstInList:
                return enemiesInRange.First();

            case TargetPriority.MostHealth:
                return enemiesInRange
                    .OrderByDescending(e => e.GetComponent<EnemyManager>()?.m_Health ?? 0f)
                    .First();

            case TargetPriority.LeastHealth:
                return enemiesInRange
                    .OrderBy(e => e.GetComponent<EnemyManager>()?.m_Health ?? float.MaxValue)
                    .First();

            case TargetPriority.Slowest:
                return enemiesInRange
                    .OrderBy(e => e.GetComponent<EnemyMovement>()?.m_Speed ?? float.MaxValue)
                    .First();

            case TargetPriority.Fastest:
                return enemiesInRange
                    .OrderByDescending(e => e.GetComponent<EnemyMovement>()?.m_Speed ?? 0f)
                    .First();

            case TargetPriority.WeakToElement:
                if (targetElements.Count == 0)
                    return enemiesInRange.First();

                return enemiesInRange
                    .Where(e => {
                        EnemyManager em = e.GetComponent<EnemyManager>();
                        if (em == null) return false;
                        return targetElements.All(element => em.IsWeakTo(element));
                    })
                    .FirstOrDefault() ?? enemiesInRange.First();

            default:
                return enemiesInRange.First();
        }
    }

    // Llamado desde la UI cuando el jugador cambia la prioridad
    [ServerRpc(RequireOwnership = false)]
    public void SetPriorityServerRpc(TargetPriority newPriority)
    {
        targetPriority.Value = newPriority;
    }

    public void RemoveFromList(GameObject obj)
    {
        enemiesInRange.Remove(obj);
    }

    // --- Gizmos ---
    private Vector3[] GetHexVertices()
    {
        Vector3[] vertices = new Vector3[6];

        for (int i = 0; i < 6; i++)
        {
            // 30f para que el hexágono tenga lados planos arriba y abajo (flat-top), igual que la torreta
            float angleDeg = 60f * i + 30f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            vertices[i] = transform.position + new Vector3(
                Mathf.Cos(angleRad) * hexSize,
                0f,
                Mathf.Sin(angleRad) * hexSize
            );
        }

        return vertices;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3[] vertices = GetHexVertices();

        // --- Las tres cajas del OverlapBox ---
        Gizmos.color = new Color(1f, 1f, 0f, 0.05f);
        Vector3 boxSize = new Vector3(HexHeight, detectionHeight, HexWidth);
        foreach (Quaternion rotation in boxRotations)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, rotation, Vector3.one);
            Gizmos.matrix = matrix;
            Gizmos.DrawCube(Vector3.zero, boxSize);
        }

        // Resetea la matriz para el resto de gizmos
        Gizmos.matrix = Matrix4x4.identity;
    }
}