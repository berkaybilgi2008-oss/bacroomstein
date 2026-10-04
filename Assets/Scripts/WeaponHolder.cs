using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon position")]
    [SerializeField] private Transform weaponSocket;
    [Header("Weapon visuals: Element 0 = Cola, Element 1 = Upright Cola")]
    [SerializeField] private GameObject[] weaponVisuals;
    [Header("Basic firing")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float range = 60f;
    [SerializeField] private float impactForce = 8f;

    private bool[] ownedWeapons;
    private int equippedIndex = -1;
    private float nextFireTime;
    private GUIStyle crosshairStyle;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = GetComponentInParent<Camera>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (weaponSocket == null) weaponSocket = transform;
        ownedWeapons = new bool[weaponVisuals == null ? 0 : weaponVisuals.Length];
        if (weaponVisuals == null) return;
        foreach (GameObject visual in weaponVisuals)
        {
            if (visual == null) continue;
            if (visual.transform.parent != weaponSocket) visual.transform.SetParent(weaponSocket, false);
            visual.SetActive(false);
        }
    }

    public void AddWeapon(int index)
    {
        if (weaponVisuals == null || index < 0 || index >= weaponVisuals.Length || weaponVisuals[index] == null)
        {
            Debug.LogWarning("WeaponHolder: Assign a weapon visual at Weapon Visuals element " + index + ".");
            return;
        }
        ownedWeapons[index] = true;
    }

    public void EquipWeapon(int index)
    {
        if (ownedWeapons == null || index < 0 || index >= ownedWeapons.Length || !ownedWeapons[index] || weaponVisuals[index] == null)
        {
            Debug.LogWarning("WeaponHolder: Collect the required pickup first, and assign its visual.");
            return;
        }
        for (int i = 0; i < weaponVisuals.Length; i++)
            if (weaponVisuals[i] != null) weaponVisuals[i].SetActive(i == index);
        equippedIndex = index;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) EquipWeapon(1);

        if (equippedIndex < 0 || playerCamera == null || !Input.GetMouseButtonDown(0) || Time.time < nextFireTime) return;
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