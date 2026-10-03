using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Prototype cola bottle with two modes:
/// Q switches between melee grip and forward soda burst.
/// Left click swings in melee mode or fires one short burst in spray mode.
/// A simple primitive is generated as a temporary visual until sprite art is ready.
/// </summary>
public class ColaBottleController : MonoBehaviour
{
    public enum BottleMode { Melee, SodaBurst }

    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform weaponPivot;

    [Header("Mode switching")]
    [SerializeField] private float switchDuration = 0.32f;
    [SerializeField] private float switchHeight = 0.22f;

    [Header("Melee")]
    [SerializeField] private float meleeRange = 2.1f;
    [SerializeField] private float meleeRadius = 0.55f;
    [SerializeField] private float meleeDamage = 35f;
    [SerializeField] private float meleeCooldown = 0.42f;

    [Header("Soda burst")]
    [SerializeField] private float sprayRange = 7f;
    [SerializeField] private float sprayRadius = 0.3f;
    [SerializeField] private float sprayDamage = 20f;
    [SerializeField] private float sprayCooldown = 0.65f;

    public BottleMode CurrentMode { get; private set; } = BottleMode.Melee;

    private bool switching;
    private float nextAttackTime;
    private Vector3 pivotBasePosition;
    private Quaternion pivotBaseRotation;
    private Transform bottleVisual;
    private Vector3 visualBaseScale;

    private void Awake()
    {
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
            else if (Camera.main != null) playerCamera = Camera.main.transform;
        }

        if (weaponPivot == null && playerCamera != null)
        {
            Transform existing = playerCamera.Find("WeaponPivot");
            if (existing != null) weaponPivot = existing;
            else
            {
                GameObject pivot = new GameObject("WeaponPivot");
                pivot.transform.SetParent(playerCamera, false);
                pivot.transform.localPosition = new Vector3(0.34f, -0.28f, 0.62f);
                pivot.transform.localRotation = Quaternion.identity;
                weaponPivot = pivot.transform;
            }
        }

        if (weaponPivot != null)
        {
            pivotBasePosition = weaponPivot.localPosition;
            pivotBaseRotation = weaponPivot.localRotation;
            CreatePlaceholderBottle();
            ApplyModePose(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame && !switching)
            StartCoroutine(SwitchMode());

        if (Mouse.current.leftButton.wasPressedThisFrame && !switching && Time.time >= nextAttackTime)
        {
            if (CurrentMode == BottleMode.Melee) StartCoroutine(MeleeSwing());
            else FireSodaBurst();
        }
    }

    private IEnumerator SwitchMode()
    {
        switching = true;
        BottleMode targetMode = CurrentMode == BottleMode.Melee ? BottleMode.SodaBurst : BottleMode.Melee;
        Vector3 startPosition = weaponPivot.localPosition;
        Quaternion startRotation = weaponPivot.localRotation;
        Vector3 targetPosition = pivotBasePosition + new Vector3(0f, switchHeight, 0f);
        Quaternion targetRotation = pivotBaseRotation * Quaternion.Euler(0f, 0f, 180f);

        float elapsed = 0f;
        while (elapsed < switchDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / switchDuration);
            float arc = Mathf.Sin(t * Mathf.PI);
            weaponPivot.localPosition = Vector3.Lerp(startPosition, targetPosition, t) + Vector3.up * (switchHeight * arc);
            weaponPivot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        CurrentMode = targetMode;
        ApplyModePose(true);
        switching = false;
    }

    private void ApplyModePose(bool animatePose)
    {
        if (weaponPivot == null) return;
        weaponPivot.localPosition = pivotBasePosition;
        weaponPivot.localRotation = pivotBaseRotation;
        if (bottleVisual != null)
        {
            // The sideways grip points the cap forward along the camera's +Z axis.
            bottleVisual.localRotation = CurrentMode == BottleMode.Melee
                ? Quaternion.Euler(0f, 0f, -35f)
                : Quaternion.Euler(0f, 0f, 90f);
        }
    }

    private IEnumerator MeleeSwing()
    {
        nextAttackTime = Time.time + meleeCooldown;
        Quaternion start = weaponPivot.localRotation;
        Quaternion strike = start * Quaternion.Euler(-25f, 0f, -48f);
        float duration = 0.16f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            weaponPivot.localRotation = Quaternion.Slerp(start, strike, elapsed / duration);
            yield return null;
        }

        Vector3 origin = playerCamera != null ? playerCamera.position : transform.position + Vector3.up;
        Vector3 direction = playerCamera != null ? playerCamera.forward : transform.forward;
        if (Physics.SphereCast(origin, meleeRadius, direction, out RaycastHit hit, meleeRange))
            ApplyDamage(hit.collider, meleeDamage);

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            weaponPivot.localRotation = Quaternion.Slerp(strike, pivotBaseRotation, elapsed / duration);
            yield return null;
        }
        weaponPivot.localRotation = pivotBaseRotation;
    }

    private void FireSodaBurst()
    {
        nextAttackTime = Time.time + sprayCooldown;
        Vector3 origin = playerCamera != null ? playerCamera.position : transform.position + Vector3.up;
        Vector3 direction = playerCamera != null ? playerCamera.forward : transform.forward;

        if (Physics.SphereCast(origin, sprayRadius, direction, out RaycastHit hit, sprayRange))
            ApplyDamage(hit.collider, sprayDamage);

        // Short visual feedback; replace with soda particles and sound during polish.
        StartCoroutine(SprayKick());
    }

    private IEnumerator SprayKick()
    {
        if (weaponPivot == null) yield break;
        Vector3 original = weaponPivot.localPosition;
        weaponPivot.localPosition = original + new Vector3(0f, 0f, -0.09f);
        yield return new WaitForSeconds(0.07f);
        weaponPivot.localPosition = original;
    }

    private void ApplyDamage(Collider target, float amount)
    {
        MonoBehaviour[] components = target.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component is IDamageable damageable)
            {
                damageable.TakeDamage(amount);
                break;
            }
        }
    }

    private void CreatePlaceholderBottle()
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "BottlePlaceholder";
        visual.transform.SetParent(weaponPivot, false);
        visual.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        visual.transform.localRotation = Quaternion.Euler(90f, 0f, -35f);
        visual.transform.localScale = new Vector3(0.11f, 0.28f, 0.11f);

        Collider collider = visual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        bottleVisual = visual.transform;
        visualBaseScale = bottleVisual.localScale;
    }
}

/// <summary>Implement this interface on enemies or breakable targets to receive damage.</summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
