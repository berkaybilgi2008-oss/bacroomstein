using UnityEngine;

public class ColaGun : MonoBehaviour
{
    [Header("Assign your own held-weapon visual in the Inspector")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject weaponVisual;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float range = 60f;
    [SerializeField] private float impactForce = 8f;

    private float nextFireTime;
    private bool unlocked;
    private GUIStyle crosshairStyle;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        // The player starts with empty hands.
        if (weaponVisual != null)
            weaponVisual.SetActive(false);
    }

    public void UnlockWeapon()
    {
        unlocked = true;
        if (weaponVisual != null)
            weaponVisual.SetActive(true);
        else
            Debug.Log("ColaGun: Cola collected. Assign your held weapon visual in the ColaGun Inspector.");
    }

    private void Update()
    {
        if (!unlocked || playerCamera == null)
            return;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    private void Fire()
    {
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
        if (!Application.isPlaying || !unlocked)
            return;

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
