using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;
    [Header("2.5D Sprite")]
    [SerializeField] private Transform spriteRoot;
    [Tooltip("Yaw offset if the front of your sprite does not face local +Z.")]
    [SerializeField] private float spriteYawOffset;
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
        if (spriteAnimation == null) spriteAnimation = GetComponentInChildren<EnemySpriteAnimation>();
        FindPlayer();
        UpdateAnimation(false, false);
    }

    private void Update()
    {
        if (player == null) FindPlayer();
        if (player == null)
        {
            ApplyGravity(Vector3.zero);
            UpdateAnimation(false, false);
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        Vector3 flatDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        float distance = flatDirection.magnitude;

        if (!hasDetectedPlayer && distance <= detectionRange)
            hasDetectedPlayer = true;

        bool moving = false;
        bool shooting = false;
        Vector3 horizontal = Vector3.zero;

        if (hasDetectedPlayer)
        {
            if (flatDirection.sqrMagnitude > 0.001f)
            {
                Quaternion desired = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Euler(0f, desired.eulerAngles.y, 0f);
                if (spriteRoot != null)
                    spriteRoot.localRotation = Quaternion.Euler(0f, spriteYawOffset, 0f);
            }

            if (distance > stopDistance && flatDirection.sqrMagnitude > 0.001f)
            {
                horizontal = flatDirection.normalized * moveSpeed;
                moving = true;
            }

            if (distance <= weaponRange && Time.time >= nextShotTime && HasLineOfSight())
            {
                nextShotTime = Time.time + 1f / Mathf.Max(0.05f, shotsPerSecond);
                if (playerHealth == null) playerHealth = player.GetComponentInParent<Health>();
                if (playerHealth != null) playerHealth.TakeDamage(damage);
                shooting = true;
                if (spriteAnimation != null) spriteAnimation.PlayShoot();
            }
        }

        ApplyGravity(horizontal);
        if (!shooting) UpdateAnimation(moving, hasDetectedPlayer);
    }

    private void ApplyGravity(Vector3 horizontalVelocity)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void FindPlayer()
    {
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null)
        {
            player = found.transform;
            playerHealth = player.GetComponentInParent<Health>();
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 destination = player.position + Vector3.up;
        Vector3 direction = destination - origin;
        float distance = direction.magnitude;
        if (distance <= 0.001f) return true;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit,
            Mathf.Min(weaponRange, distance + 0.1f), lineOfSightMask, QueryTriggerInteraction.Ignore))
            return true;

        return hit.transform == player || hit.transform.IsChildOf(player);
    }

    private void UpdateAnimation(bool moving, bool detected)
    {
        if (spriteAnimation == null) return;
        if (moving) spriteAnimation.PlayWalk();
        else spriteAnimation.PlayIdle();
    }
}