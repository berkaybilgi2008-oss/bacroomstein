using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ColaPickup : MonoBehaviour
{
    private Transform playerCamera;
    private bool collected;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void LateUpdate()
    {
        if (collected) return;

        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) playerCamera = mainCamera.transform;
        }

        if (playerCamera != null)
        {
            Vector3 lookDirection = playerCamera.forward;
            lookDirection.y -= 0.25f;
            transform.forward = lookDirection.normalized;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        Transform playerRoot = other.transform.root;
        if (!other.CompareTag("Player") && !playerRoot.CompareTag("Player"))
            return;

        WeaponHolder weaponHolder = FindFirstObjectByType<WeaponHolder>();
        if (weaponHolder == null)
        {
            Debug.LogWarning("ColaPickup: Add WeaponHolder to the Player or FPS camera before collecting the cola.");
            return;
        }

        collected = true;
        weaponHolder.EquipWeapon(0);
        Destroy(gameObject);
    }
}
