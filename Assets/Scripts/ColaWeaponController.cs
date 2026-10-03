using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cola weapon gameplay only. Intentionally creates no placeholder art; the player can add their own sprites.
/// Q switches between club and soda-burst modes. Left click attacks once per press.
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

    [Header("Feedback")]
    [SerializeField] private float feedbackDuration = 0.14f;

    private BottleMode mode = BottleMode.Club;
    private float nextAttackTime;
    private float feedbackUntil;
    private float hitUntil;
    private bool lastAttackHit;
    private string statusMessage = "CLUB";
    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            mode = mode == BottleMode.Club ? BottleMode.Soda : BottleMode.Club;
            statusMessage = mode == BottleMode.Club ? "CLUB MODE" : "SODA BURST MODE";
            feedbackUntil = Time.unscaledTime + 0.65f;
            Debug.Log("Cola weapon mode: " + mode);
        }

        if (Cursor.lockState != CursorLockMode.Locked ||
            !Mouse.current.leftButton.wasPressedThisFrame ||
            Time.time < nextAttackTime)
            return;

        if (mode == BottleMode.Club)
            Attack(meleeRange, meleeDamage, meleeCooldown, "SWING");
        else
            Attack(sodaRange, sodaDamage, sodaCooldown, "SODA BURST");
    }

    private void Attack(float range, float damage, float cooldown, string attackName)
    {
        nextAttackTime = Time.time + cooldown;
        feedbackUntil = Time.unscaledTime + feedbackDuration;
        lastAttackHit = false;
        statusMessage = attackName;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, range, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                // Ignore the player's own colliders and continue looking for the first real target.
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                lastAttackHit = ApplyDamage(hit.collider, damage);
                if (lastAttackHit)
                {
                    hitUntil = Time.unscaledTime + 0.18f;
                    statusMessage = attackName + " - HIT!";
                }
                else
                {
                    statusMessage = attackName + " - NO TARGET";
                }
                break;
            }
        }

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(AttackFeedback());

        Debug.Log("Cola " + attackName + (lastAttackHit ? ": HIT" : ": MISS / no damage receiver"));
    }

    private IEnumerator AttackFeedback()
    {
        // A small UI pulse confirms every click, even before the weapon sprites are added.
        float end = Time.unscaledTime + feedbackDuration;
        while (Time.unscaledTime < end)
            yield return null;
        feedbackRoutine = null;
    }

    private static bool ApplyDamage(Collider target, float amount)
    {
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
                return true;
            }
        }
        return false;
    }

    private void OnGUI()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        bool attacking = Time.unscaledTime < feedbackUntil;
        bool hit = Time.unscaledTime < hitUntil;

        GUI.color = hit ? Color.red : (attacking ? new Color(1f, 0.85f, 0.35f) : Color.white);
        float gap = attacking ? 8f : 5f;
        float arm = attacking ? 8f : 5f;
        GUI.DrawTexture(new Rect(cx - gap - arm, cy - 1f, arm, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx + gap, cy - 1f, arm, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1f, cy - gap - arm, 2f, arm), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1f, cy + gap, 2f, arm), Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(18f, Screen.height - 48f, 500f, 24f),
            mode == BottleMode.Club
                ? "COLA: CLUB  |  Q: SWITCH  |  LMB: SWING"
                : "COLA: SODA BURST  |  Q: SWITCH  |  LMB: FIRE");
        if (Time.unscaledTime < feedbackUntil)
            GUI.Label(new Rect(cx + 18f, cy + 12f, 260f, 24f), statusMessage);
        GUI.color = Color.white;
    }
}
