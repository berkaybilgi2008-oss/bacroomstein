using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;

    [Header("2.5D Visual")]
    [Tooltip("The child object containing the SpriteRenderer / EnemySpriteAnimation.")]
    [SerializeField] private Transform spriteRoot;
    [Tooltip("Use 0 if the sprite's front faces local +Z. Adjust only if the art faces another direction.")]
    [SerializeField] private float spriteYawOffset = 0f;

    [Header("Detection and movement")]
    [Min(0.1f)] [SerializeField] private float detectionRange = 25f;
    [Min(0f)] [SerializeField] private float moveSpeed = 3.5f;
    [Min(0.1f)] [SerializeField] private float stopDistance = 8f;
    [Min(0f)] [SerializeField] private float gravity = 20f;

    [Header("Shooting")]
    [Min(0f)] [SerializeField] private float damage = 10f;
    [Min(0.05f)] [SerializeField] private float shotsPerSecond = 1f;
    [Min(0.1f)] [SerializeField] private float weaponRange = 30f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [SerializeField] private EnemySpriteAnimation spriteAnimation;

    private CharacterController controller;
    private Health playerHealth;
    private float verticalVelocity;
    private float nextShotTime;
    private bool hasDetectedPlayer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (spriteRoot == null && transform.childCount > 0)
            spriteRoot = transform.GetChild(0);

        if (spriteAnimation == null)
            spriteAnimation = GetComponentInChildren<EnemySpriteAnimation>();

        FindPlayer();
        UpdateAnimation(false);
    }

    private void Update()
    {
        if (player == null)
            FindPlayer();

        Vector3 horizontalVelocity = Vector3.zero;
        bool moving = false;

        if (player != null)
        {
            // Measure distance and direction on the ground plane only.
            Vector3 toPlayer = player.position - transform.position;
            Vector3 flatToPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
            float distance = flatToPlayer.magnitude;

            if (distance <= detectionRange)
                hasDetectedPlayer = true;

            // The body never pitches or turns. Only the sprite child faces the player,
            // rotating around world-up so it stays upright.
            if (spriteRoot != null && flatToPlayer.sqrMagnitude > 0.0001f)
            {
                Quaternion facing = Quaternion.LookRotation(flatToPlayer.normalized, Vector3.up);
                spriteRoot.rotation = Quaternion.Euler(0f, facing.eulerAngles.y + spriteYawOffset, 0f);
            }

            if (hasDetectedPlayer && distance > stopDistance && flatToPlayer.sqrMagnitude > 0.0001f)
            {
                horizontalVelocity = flatToPlayer.normalized * moveSpeed;
                moving = true;
            }

            if (hasDetectedPlayer &&
                distance <= weaponRange &&
                Time.time >= nextShotTime &&
                HasLineOfSight())
            {
                nextShotTime = Time.time + 1f / Mathf.Max(0.05f, shotsPerSecond);

                if (playerHealth == null)
                    playerHealth = player.GetComponentInParent<Health>();

                if (playerHealth != null)
                    playerHealth.TakeDamage(damage);

                if (spriteAnimation != null)
                    spriteAnimation.PlayShoot();
            }
        }

        ApplyGravity(horizontalVelocity);
        UpdateAnimation(moving);
    }

    private void ApplyGravity(Vector3 horizontalVelocity)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    private void FindPlayer()
    {
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found == null)
            return;

        player = found.transform;
        playerHealth = player.GetComponentInParent<Health>();
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 destination = player.position + Vector3.up;
        Vector3 direction = destination - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return true;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit,
                Mathf.Min(weaponRange, distance + 0.1f), lineOfSightMask,
                QueryTriggerInteraction.Ignore))
            return true;

        return hit.transform == player || hit.transform.IsChildOf(player);
    }

    private void UpdateAnimation(bool moving)
    {
        if (spriteAnimation == null)
            return;

        if (moving)
            spriteAnimation.PlayWalk();
        else
            spriteAnimation.PlayIdle();
    }
}