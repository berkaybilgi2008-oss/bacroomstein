using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ColaPickup : MonoBehaviour
{
    [SerializeField, Min(0)] private int weaponIndex = 0;

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
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

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
        if (!other.CompareTag("Player") && !playerRoot.CompareTag("Player")) return;

        WeaponHolder holder = FindFirstObjectByType<WeaponHolder>();
        if (holder == null)
        {
            Debug.LogWarning("ColaPickup: No WeaponHolder found in the scene.");
            return;
        }

        collected = true;
        holder.AddWeapon(weaponIndex);
        Destroy(gameObject);
    }
}