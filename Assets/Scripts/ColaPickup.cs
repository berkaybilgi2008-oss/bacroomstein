using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ColaPickup : MonoBehaviour
{
    private Transform playerCamera;
    private bool collected;

    private void Awake()
    {
        BoxCollider pickupCollider = GetComponent<BoxCollider>();
        pickupCollider.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            playerCamera = mainCamera.transform;
    }

    private void LateUpdate()
    {
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerCamera = mainCamera.transform;
        }

        if (playerCamera != null)
        {
            // Keep the flat sprite facing the camera/player.
            transform.forward = playerCamera.forward;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;
        Destroy(gameObject);
    }
}