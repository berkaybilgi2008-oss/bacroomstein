using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stopDistance = 6f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float gravity = 20f;

    private CharacterController controller;
    private float verticalVelocity;
    private bool detectedPlayer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            ApplyGravity(Vector3.zero);
            return;
        }

        // Enemy AI uses only the player's world position.
        // It never reads WASD/input from the player.
        Vector3 offset = player.position - transform.position;
        Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z);
        float distance = flatOffset.magnitude;

        if (!detectedPlayer && distance <= detectionRange)
            detectedPlayer = true;

        Vector3 horizontalVelocity = Vector3.zero;

        if (detectedPlayer && distance > stopDistance)
        {
            horizontalVelocity = flatOffset.normalized * moveSpeed;
        }

        // Rotate the whole enemy only around Y.
        // No pitch and no roll.
        if (detectedPlayer && flatOffset.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(
                flatOffset.normalized,
                Vector3.up
            );
        }

        ApplyGravity(horizontalVelocity);
    }

    private void ApplyGravity(Vector3 horizontalVelocity)
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 velocity = horizontalVelocity;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }
}