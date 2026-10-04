using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform muzzle;
    [Header("Detection and movement")]
    [Min(1f)] [SerializeField] private float detectionRange = 25f;
    [Min(0f)] [SerializeField] private float moveSpeed = 3.5f;
    [Min(0.1f)] [SerializeField] private float stopDistance = 8f;
    [Header("Weapon")]
    [Min(0f)] [SerializeField] private float damage = 10f;
    [Min(0.05f)] [SerializeField] private float shotsPerSecond = 1f;
    [Min(1f)] [SerializeField] private float weaponRange = 30f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [Header("Death drop")]
    [SerializeField] private GameObject fixedWeaponDropPrefab;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropUpwardForce = 2f;

    private Health playerHealth;
    private float nextShotTime;
    private Health ownHealth;

    private void Awake()
    {
        ownHealth = GetComponent<Health>();
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
        if (player != null) playerHealth = player.GetComponentInParent<Health>();
    }

    private void Update()
    {
        if (player == null) return;
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange) return;

        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (flat.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 8f * Time.deltaTime);
        }

        if (distance > stopDistance)
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        if (Time.time >= nextShotTime && distance <= weaponRange && HasLineOfSight())
        {
            nextShotTime = Time.time + 1f / Mathf.Max(0.05f, shotsPerSecond);
            if (playerHealth == null) playerHealth = player.GetComponentInParent<Health>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 destination = player.position + Vector3.up * 1f;
        Vector3 direction = destination - origin;
        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, Mathf.Min(weaponRange, direction.magnitude + 0.1f), lineOfSightMask, QueryTriggerInteraction.Ignore))
            return true;
        return hit.transform == player || hit.transform.IsChildOf(player);
    }

    public void Die()
    {
        if (fixedWeaponDropPrefab == null) return;
        Vector3 position = dropPoint != null ? dropPoint.position : transform.position + Vector3.up * 0.5f;
        GameObject drop = Instantiate(fixedWeaponDropPrefab, position, Quaternion.identity);
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(Vector3.up * dropUpwardForce, ForceMode.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}