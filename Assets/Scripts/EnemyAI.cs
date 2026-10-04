using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform muzzle;

    [Header("2.5D Sprite")]
    [Tooltip("Assign the child containing your sprite/Animator. Leave empty if the sprite should not billboard.")]
    [SerializeField] private Transform spriteRoot;
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Camera targetCamera;

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
    [Min(0f)] [SerializeField] private float dropForwardDistance = 1.2f;
    [SerializeField] private float dropUpwardForce = 0.5f;
    [SerializeField] private LayerMask groundMask = ~0;

    private Health playerHealth;
    private Health ownHealth;
    private float nextShotTime;
    private bool dead;

    private void Awake()
    {
        ownHealth = GetComponent<Health>();
        if (targetCamera == null) targetCamera = Camera.main;

        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (player != null) playerHealth = player.GetComponentInParent<Health>();
    }

    private void LateUpdate()
    {
        if (dead || !faceCamera || spriteRoot == null) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        // Rotate only the sprite child, never the movement/collider root.
        Vector3 direction = spriteRoot.position - targetCamera.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            spriteRoot.rotation = Quaternion.LookRotation(direction);
    }

    private void Update()
    {
        if (dead || player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float distance = flatToPlayer.magnitude;
        if (distance > detectionRange) return;

        // Movement stays on the XZ ground plane for a 3D environment with 2D sprites.
        if (distance > stopDistance && distance > 0.001f)
            transform.position += flatToPlayer.normalized * (moveSpeed * Time.deltaTime);

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
        float distance = direction.magnitude;
        if (distance <= 0.001f) return true;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit,
            Mathf.Min(weaponRange, distance + 0.1f), lineOfSightMask, QueryTriggerInteraction.Ignore))
            return true;

        return hit.transform == player || hit.transform.IsChildOf(player);
    }

    // Connect this method to Health -> On Death in the Inspector.
    // Set Health.destroyOnDeath to false so the corpse can remain in the scene.
    public void Die()
    {
        if (dead) return;
        dead = true;
        DropWeapon();
        // Keep this GameObject and its sprite/Animator in the scene for your death animation.
        // Your animation can disable colliders or switch to a corpse state when ready.
    }

    private void DropWeapon()
    {
        if (fixedWeaponDropPrefab == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;

        Vector3 position = dropPoint != null
            ? dropPoint.position
            : transform.position + forward * dropForwardDistance + Vector3.up * 0.15f;

        // Snap the drop to the floor when a ground collider is available.
        Vector3 rayOrigin = position + Vector3.up * 2f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, 5f, groundMask, QueryTriggerInteraction.Ignore))
            position.y = groundHit.point.y + 0.05f;

        GameObject drop = Instantiate(fixedWeaponDropPrefab, position, Quaternion.identity);
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null && dropUpwardForce > 0f)
            rb.AddForce(Vector3.up * dropUpwardForce, ForceMode.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + forward * dropForwardDistance);
    }
}