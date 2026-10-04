using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WeaponHolder : MonoBehaviour
{
    [Header("Weapon position")]
    [SerializeField] private Transform weaponSocket;
    [Header("Weapon visuals: Element 0 = Cola, Element 1 = Upright Cola")]
    [SerializeField] private GameObject[] weaponVisuals;
    [Header("Ranged weapon settings")]
    [Min(0f)] [SerializeField] private float rangedDamage = 15f;
    [Min(0.01f)] [SerializeField] private float fireRate = 0.25f;
    [Min(0.1f)] [SerializeField] private float range = 60f;
    [SerializeField] private float impactForce = 8f;
    [Header("Upright Cola melee settings")]
    [Min(0f)] [SerializeField] private float meleeDamage = 40f;
    [Min(0.1f)] [SerializeField] private float meleeReach = 1.6f;
    [Min(0.1f)] [SerializeField] private float meleeRadius = 0.75f;
    [Header("Collision")]
    [SerializeField] private LayerMask hitMask = ~0;

    private bool[] ownedWeapons;
    private int equippedIndex = -1;
    private float nextFireTime;
    private GUIStyle crosshairStyle;

    private void Awake()
    {
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
        if (WeaponKeyPressed(1)) EquipWeapon(1);
        else if (WeaponKeyPressed(2)) EquipWeapon(0);
        if (equippedIndex < 0 || !FirePressed() || Time.time < nextFireTime) return;
        nextFireTime = Time.time + Mathf.Max(0.01f, fireRate);
        if (weaponVisuals != null && equippedIndex < weaponVisuals.Length && weaponVisuals[equippedIndex] != null)
        {
            ColaWeaponAnimation anim = weaponVisuals[equippedIndex].GetComponentInChildren<ColaWeaponAnimation>();
            if (anim != null) anim.PlayFireAnimation();
        }
        Camera cam = GetPlayerCamera();
        if (cam == null) return;
        if (equippedIndex == 1) Swing(cam);
        else Shoot(cam);
    }

    private Camera GetPlayerCamera()
    {
        Camera c = GetComponentInChildren<Camera>();
        if (c == null) c = GetComponentInParent<Camera>();
        if (c == null) c = Camera.main;
        return c;
    }

    private void Shoot(Camera cam)
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore)) return;
        Health health = hit.collider.GetComponentInParent<Health>();
        if (health != null && health.transform.root != transform.root) health.TakeDamage(rangedDamage);
        Rigidbody body = hit.rigidbody;
        if (body != null && !body.isKinematic && body.transform.root != transform.root)
            body.AddForceAtPosition(ray.direction * impactForce, hit.point, ForceMode.Impulse);
    }

    private void Swing(Camera cam)
    {
        Vector3 center = cam.transform.position + cam.transform.forward * meleeReach;
        Collider[] targets = Physics.OverlapSphere(center, meleeRadius, hitMask, QueryTriggerInteraction.Ignore);
        foreach (Collider target in targets)
        {
            if (target.transform.root == transform.root) continue;
            Health health = target.GetComponentInParent<Health>();
            if (health != null) health.TakeDamage(meleeDamage);
            Rigidbody body = target.attachedRigidbody;
            if (body != null && !body.isKinematic)
                body.AddForce((body.worldCenterOfMass - cam.transform.position).normalized * impactForce, ForceMode.Impulse);
        }
    }

    private bool WeaponKeyPressed(int number)
    {
        bool pressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed = number == 1 ? Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) : Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
#endif
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (number == 1) pressed |= Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame;
            else pressed |= Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame;
        }
#endif
        return pressed;
    }

    private bool FirePressed()
    {
        bool pressed = false;
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed = Input.GetMouseButtonDown(0);
#endif
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) pressed |= Mouse.current.leftButton.wasPressedThisFrame;
#endif
        return pressed;
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || equippedIndex < 0) return;
        if (crosshairStyle == null)
        {
            crosshairStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 22 };
            crosshairStyle.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(Screen.width * 0.5f - 12f, Screen.height * 0.5f - 12f, 24f, 24f), "+", crosshairStyle);
    }
}