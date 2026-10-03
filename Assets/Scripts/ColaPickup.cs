using UnityEngine;

/// <summary>
/// A 2D pickup that faces the player camera and automatically grants the cola on contact.
/// </summary>
public class ColaPickup : MonoBehaviour
{
    private ColaWeaponController owner;
    private Camera targetCamera;
    private SpriteRenderer spriteRenderer;

    public void Configure(ColaWeaponController controller, Camera cameraToFace, Texture2D pickupTexture)
    {
        owner = controller;
        targetCamera = cameraToFace;

        GameObject visual = new GameObject("Pickup Sprite");
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        visual.transform.localScale = Vector3.one * 0.012f;
        spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 10;

        if (pickupTexture != null)
            spriteRenderer.sprite = Sprite.Create(pickupTexture,
                new Rect(0, 0, pickupTexture.width, pickupTexture.height),
                new Vector2(0.5f, 0.5f), 100f);

        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.85f;
        Rigidbody body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;
        Vector3 direction = targetCamera.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (owner == null) return;
        ColaWeaponController playerWeapon = other.GetComponentInParent<ColaWeaponController>();
        if (playerWeapon != owner) return;
        owner.CollectCola();
    }
}
