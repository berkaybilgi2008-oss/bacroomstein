using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon position")]
    [Tooltip("Leave empty to use this WeaponHolder object's Transform.")]
    [SerializeField] private Transform weaponSocket;

    [Header("Weapon visuals (element 0 is the ground cola weapon)")]
    [Tooltip("Add each held weapon object here. Element 0 must be ColaWeapon.")]
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

        if (weaponSocket == null)
            weaponSocket = transform;

        // First hide every visual under the holder, even if the Inspector array was forgotten.
        for (int i = 0; i < weaponSocket.childCount; i++)
            weaponSocket.GetChild(i).gameObject.SetActive(false);

        // Ensure listed visuals are parented to the holder and remain hidden at game start.
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
            Debug.LogWarning("WeaponHolder: Add ColaWeapon to Weapon Visuals element 0 in the Inspector.");
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