using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon position")]
    [Tooltip("Leave empty to use this WeaponHolder object's Transform.")]
    [SerializeField] private Transform weaponSocket;

    [Header("Weapon visuals (element 0 is the ground cola weapon)")]
    [Tooltip("Add every held weapon object here. All are hidden until picked up.")]
    [SerializeField] private GameObject[] weaponVisuals;

    [Header("Basic firing")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float range = 60f;
    [SerializeField] private float impactForce = 8f;

    private int equippedIndex = -1;
    private float nextFireTime;
    private GUIStyle crosshairStyle;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInParent<Camera>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Keep weapons attached to the holder, not directly to the camera.
        if (weaponSocket == null)
            weaponSocket = transform;

        // Empty hands at game start. Make sure every listed visual is disabled.
        if (weaponVisuals == null) return;
        foreach (GameObject visual in weaponVisuals)
        {
            if (visual == null) continue;
            if (visual.transform.parent != weaponSocket)
                visual.transform.SetParent(weaponSocket, false);
            visual.SetActive(false);
        }
    }

    public void EquipWeapon(int index)
    {
        if (weaponVisuals == null || index < 0 || index >= weaponVisuals.Length || weaponVisuals[index] == null)
        {
            Debug.LogWarning("WeaponHolder: Add the weapon visual to the Weapon Visuals list; index " + index + " is missing.");
            return;
        }

        for (int i = 0; i < weaponVisuals.Length; i++)
        {
            if (weaponVisuals[i] != null)
                weaponVisuals[i].SetActive(i == index);
        }

        equippedIndex = index;
    }

    private void Update()
    {
        if (equippedIndex < 0 || playerCamera == null || !Input.GetMouseButtonDown(0) || Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + fireRate;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null && !hitBody.isKinematic)
                hitBody.AddForceAtPosition(ray.direction * impactForce, hit.point, ForceMode.Impulse);
        }
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || equippedIndex < 0) return;
        if (crosshairStyle == null)
        {
            crosshairStyle = new GUIStyle(GUI.skin.label);
            crosshairStyle.alignment = TextAnchor.MiddleCenter;
            crosshairStyle.fontSize = 22;
            crosshairStyle.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(Screen.width * 0.5f - 12f, Screen.height * 0.5f - 12f, 24f, 24f), "+", crosshairStyle);
    }
}