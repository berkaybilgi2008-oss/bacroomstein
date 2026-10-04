using UnityEngine;

public class ColaGun : MonoBehaviour
{
    [Header("FPS Cola Gun")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Vector3 localPosition = new Vector3(0.32f, -0.28f, 0.65f);
    [SerializeField] private Vector3 localRotation = new Vector3(0f, 0f, -8f);
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float range = 60f;
    [SerializeField] private float impactForce = 8f;

    private float nextFireTime;
    private GameObject gunVisual;
    private GUIStyle crosshairStyle;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogWarning("ColaGun: No player camera found. Assign the FPS camera in the Inspector.");
            enabled = false;
            return;
        }

        CreateGunVisual();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (gunVisual != null)
        {
            gunVisual.transform.localPosition = localPosition;
            gunVisual.transform.localRotation = Quaternion.Euler(localRotation);
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    private void CreateGunVisual()
    {
        gunVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gunVisual.name = "Cola Gun (FPS Visual)";
        gunVisual.transform.SetParent(playerCamera.transform, false);
        gunVisual.transform.localPosition = localPosition;
        gunVisual.transform.localRotation = Quaternion.Euler(localRotation);
        gunVisual.transform.localScale = new Vector3(0.13f, 0.22f, 0.13f);

        Collider visualCollider = gunVisual.GetComponent<Collider>();
        if (visualCollider != null)
            Destroy(visualCollider);

        Renderer renderer = gunVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (material.shader == null)
                material.shader = Shader.Find("Standard");
            material.color = new Color(0.72f, 0.08f, 0.08f);
            renderer.material = material;
        }

        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        top.name = "Cola Gun Silver Top";
        top.transform.SetParent(gunVisual.transform, false);
        top.transform.localPosition = new Vector3(0f, 0.92f, 0f);
        top.transform.localRotation = Quaternion.identity;
        top.transform.localScale = new Vector3(1.02f, 0.08f, 1.02f);
        Collider topCollider = top.GetComponent<Collider>();
        if (topCollider != null)
            Destroy(topCollider);
        Renderer topRenderer = top.GetComponent<Renderer>();
        if (topRenderer != null)
        {
            Material topMaterial = new Material(Shader.Find("Standard"));
            topMaterial.color = new Color(0.72f, 0.74f, 0.76f);
            topRenderer.material = topMaterial;
        }
    }

    private void Fire()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null && !hitBody.isKinematic)
                hitBody.AddForceAtPosition(ray.direction * impactForce, hit.point, ForceMode.Impulse);

            // Leave the existing level objects unchanged; this first version is a simple hitscan.
        }
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
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
