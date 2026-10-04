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

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Cola weapon: Main Camera tag is missing.");
            return;
        }

        SpriteRenderer pickupRenderer = GetComponent<SpriteRenderer>();
        if (pickupRenderer == null || pickupRenderer.sprite == null)
        {
            Debug.LogWarning("Cola weapon: Cola needs a SpriteRenderer with a sprite assigned.");
            return;
        }

        collected = true;
        playerCamera = mainCamera.transform;

        GameObject weapon = new GameObject("ColaWeaponView");
        weapon.transform.SetParent(playerCamera, false);
        weapon.transform.localPosition = new Vector3(0.42f, -0.32f, 0.8f);
        weapon.transform.localRotation = Quaternion.identity;
        weapon.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);

        SpriteRenderer weaponRenderer = weapon.AddComponent<SpriteRenderer>();
        weaponRenderer.sprite = pickupRenderer.sprite;
        weaponRenderer.sortingOrder = 100;

        ColaWeaponFire fire = weapon.AddComponent<ColaWeaponFire>();
        fire.SetCamera(playerCamera);

        Destroy(gameObject);
    }
}

public class ColaWeaponFire : MonoBehaviour
{
    private Transform playerCamera;
    private float nextShotTime;
    private const float FireCooldown = 0.25f;
    private const float Range = 100f;

    public void SetCamera(Transform cameraTransform)
    {
        playerCamera = cameraTransform;
    }

    private void Update()
    {
        if (playerCamera == null || !Input.GetMouseButtonDown(0) || Time.time < nextShotTime)
            return;

        nextShotTime = Time.time + FireCooldown;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            // Basic first-person hitscan: currently logs what the Cola shot hits.
            Debug.Log("Cola shot hit: " + hit.collider.name);
        }
    }
}