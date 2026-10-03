using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Temporary procedural cola-bottle weapon. Art can be replaced with transparent sprites later.
/// Q switches between a close-range club grip and a short, powerful soda burst.
/// </summary>
public class ColaWeaponController : MonoBehaviour
{
    private enum BottleMode { Club, Soda }

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Club")]
    [SerializeField] private float meleeRange = 2.1f;
    [SerializeField] private float meleeDamage = 35f;
    [SerializeField] private float meleeCooldown = 0.48f;

    [Header("Soda burst")]
    [SerializeField] private float sodaRange = 9f;
    [SerializeField] private float sodaDamage = 22f;
    [SerializeField] private float sodaCooldown = 0.7f;

    [Header("Switching")]
    [SerializeField] private float switchDuration = 0.32f;

    private BottleMode mode = BottleMode.Club;
    private bool switching;
    private float nextAttackTime;
    private Transform bottleVisual;
    private Vector3 bottleClubPosition = new Vector3(0.34f, -0.29f, 0.62f);
    private Vector3 bottleSodaPosition = new Vector3(0.30f, -0.23f, 0.62f);
    private Quaternion clubRotation = Quaternion.Euler(0f, 0f, -18f);
    private Quaternion sodaRotation = Quaternion.Euler(0f, 0f, -90f);

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        CreateTemporaryBottle();
        ApplyBottlePose(false);
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame && !switching)
            StartCoroutine(SwitchMode());

        if (!switching && Cursor.lockState == CursorLockMode.Locked &&
            Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            if (mode == BottleMode.Club)
                SwingBottle();
            else
                FireSodaBurst();
        }
    }

    private void CreateTemporaryBottle()
    {
        if (playerCamera == null)
            return;

        bottleVisual = new GameObject("Temporary Cola Bottle (replace with sprite)").transform;
        bottleVisual.SetParent(playerCamera.transform, false);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Bottle Body";
        body.transform.SetParent(bottleVisual, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localRotation = Quaternion.identity;
        body.transform.localScale = new Vector3(0.105f, 0.24f, 0.105f);
        RemoveCollider(body);
        SetColor(body, new Color(0.08f, 0.28f, 0.12f));

        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Bottle Neck";
        neck.transform.SetParent(bottleVisual, false);
        neck.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        neck.transform.localScale = new Vector3(0.045f, 0.07f, 0.045f);
        RemoveCollider(neck);
        SetColor(neck, new Color(0.12f, 0.35f, 0.16f));

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "Bottle Cap";
        cap.transform.SetParent(bottleVisual, false);
        cap.transform.localPosition = new Vector3(0f, 0.33f, 0f);
        cap.transform.localScale = new Vector3(0.05f, 0.025f, 0.05f);
        RemoveCollider(cap);
        SetColor(cap, new Color(0.85f, 0.12f, 0.08f));
    }

    private static void RemoveCollider(GameObject part)
    {
        Collider col = part.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }

    private static void SetColor(GameObject part, Color color)
    {
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer == null) return;
        renderer.material.color = color;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private IEnumerator SwitchMode()
    {
        switching = true;
        BottleMode target = mode == BottleMode.Club ? BottleMode.Soda : BottleMode.Club;
        float elapsed = 0f;
        Vector3 startPosition = bottleVisual != null ? bottleVisual.localPosition : Vector3.zero;
        Quaternion startRotation = bottleVisual != null ? bottleVisual.localRotation : Quaternion.identity;
        Vector3 targetPosition = target == BottleMode.Club ? bottleClubPosition : bottleSodaPosition;
        Quaternion targetRotation = target == BottleMode.Club ? clubRotation : sodaRotation;

        while (elapsed < switchDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / switchDuration);
            // Brief toss, then catch in the alternate grip.
            float toss = Mathf.Sin(t * Mathf.PI) * 0.16f;
            if (bottleVisual != null)
            {
                bottleVisual.localPosition = Vector3.Lerp(startPosition, targetPosition, t) + Vector3.up * toss;
                bottleVisual.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            }
            yield return null;
        }

        mode = target;
        ApplyBottlePose(false);
        switching = false;
    }

    private void ApplyBottlePose(bool animate)
    {
        if (bottleVisual == null) return;
        bottleVisual.localPosition = mode == BottleMode.Club ? bottleClubPosition : bottleSodaPosition;
        bottleVisual.localRotation = mode == BottleMode.Club ? clubRotation : sodaRotation;
    }

    private void SwingBottle()
    {
        nextAttackTime = Time.time + meleeCooldown;
        if (playerCamera != null && Physics.Raycast(playerCamera.transform.position,
            playerCamera.transform.forward, out RaycastHit hit, meleeRange))
        {
            ApplyDamage(hit.collider, meleeDamage);
        }
        Debug.Log("Cola bottle: CLUB swing");
    }

    private void FireSodaBurst()
    {
        nextAttackTime = Time.time + sodaCooldown;
        if (playerCamera != null && Physics.Raycast(playerCamera.transform.position,
            playerCamera.transform.forward, out RaycastHit hit, sodaRange))
        {
            ApplyDamage(hit.collider, sodaDamage);
        }
        Debug.Log("Cola bottle: SODA BURST");
    }

    private static void ApplyDamage(Collider target, float amount)
    {
        // Works with any future enemy component exposing TakeDamage(float).
        MonoBehaviour[] components = target.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component == null) continue;
            var method = component.GetType().GetMethod("TakeDamage",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (method != null && method.GetParameters().Length == 1 &&
                method.GetParameters()[0].ParameterType == typeof(float))
            {
                method.Invoke(component, new object[] { amount });
                return;
            }
        }
    }

    private void OnGUI()
    {
        string label = mode == BottleMode.Club
            ? "COLA BOTTLE  |  CLUB  |  Q: SWITCH  |  LMB: SWING"
            : "COLA BOTTLE  |  SODA BURST  |  Q: SWITCH  |  LMB: FIRE";
        GUI.color = new Color(1f, 0.9f, 0.65f);
        GUI.Label(new Rect(18f, Screen.height - 42f, 600f, 28f), label);
        GUI.color = Color.white;
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.Label(new Rect(cx - 4f, cy - 10f, 20f, 20f), "+");
    }
}
