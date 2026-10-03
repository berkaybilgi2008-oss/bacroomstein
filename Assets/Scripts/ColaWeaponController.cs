using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the pickup, the two cola modes, sprite animation and the existing prototype hit checks.
/// Sprite PNGs are loaded from Assets/Resources/Weapons/Cola/ at runtime.
/// </summary>
public class ColaWeaponController : MonoBehaviour
{
    private enum BottleMode { Club, Soda }

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Pickup")]
    [SerializeField] private bool spawnPickupNearPlayerAtStart = true;
    [SerializeField, Min(1f)] private float pickupSpawnDistance = 2.5f;

    [Header("Held sprite placement")]
    [SerializeField] private Vector3 heldSpriteLocalPosition = new Vector3(0.42f, -0.38f, 1.2f);
    [SerializeField, Min(0.001f)] private float heldSpriteScale = 0.05f;

    [Header("Club")]
    [SerializeField] private float meleeRange = 2.1f;
    [SerializeField] private float meleeDamage = 35f;
    [SerializeField] private float meleeCooldown = 0.48f;
    [SerializeField, Min(0.01f)] private float clubFrameDuration = 0.10f;

    [Header("Soda burst")]
    [SerializeField] private float sodaRange = 9f;
    [SerializeField] private float sodaDamage = 22f;
    [SerializeField] private float sodaCooldown = 0.7f;
    [SerializeField, Min(0.01f)] private float sodaAttackDuration = 0.14f;

    private BottleMode mode = BottleMode.Club;
    private bool hasCola;
    private float nextAttackTime;
    private float feedbackUntil;
    private float hitUntil;
    private bool lastAttackHit;
    private string statusMessage = "FIND THE COLA";
    private SpriteRenderer heldRenderer;
    private Coroutine animationRoutine;
    private Texture2D clubIdleTexture;
    private Texture2D clubUpTexture;
    private Texture2D clubDownTexture;
    private Texture2D sodaIdleTexture;
    private Texture2D sodaFireTexture;
    private Sprite clubIdleSprite;
    private Sprite clubUpSprite;
    private Sprite clubDownSprite;
    private Sprite sodaIdleSprite;
    private Sprite sodaFireSprite;
    private GameObject pickupObject;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        LoadTextures();
        CreateHeldVisual();
    }

    private void Start()
    {
        if (spawnPickupNearPlayerAtStart && !hasCola)
            SpawnPickup();
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SetMode(BottleMode.Club);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SetMode(BottleMode.Soda);

        if (!hasCola || Cursor.lockState != CursorLockMode.Locked ||
            !Mouse.current.leftButton.wasPressedThisFrame || Time.time < nextAttackTime)
            return;

        if (mode == BottleMode.Club)
        {
            nextAttackTime = Time.time + meleeCooldown;
            PlayClubAttack();
            Attack(meleeRange, meleeDamage, "SWING");
        }
        else
        {
            nextAttackTime = Time.time + sodaCooldown;
            PlaySodaAttack();
            Attack(sodaRange, sodaDamage, "SODA BURST");
        }
    }

    private void LoadTextures()
    {
        clubIdleTexture = Resources.Load<Texture2D>("Weapons/Cola/dik");
        clubUpTexture = Resources.Load<Texture2D>("Weapons/Cola/kalkik");
        clubDownTexture = Resources.Load<Texture2D>("Weapons/Cola/inik");
        sodaIdleTexture = Resources.Load<Texture2D>("Weapons/Cola/elde");
        sodaFireTexture = Resources.Load<Texture2D>("Weapons/Cola/ates");
    }

    private Sprite ToSprite(Texture2D texture)
    {
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    private void CreateHeldVisual()
    {
        if (playerCamera == null) return;

        GameObject visual = new GameObject("Held Cola Sprite");
        visual.transform.SetParent(playerCamera.transform, false);
        visual.transform.localPosition = heldSpriteLocalPosition;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * heldSpriteScale;

        heldRenderer = visual.AddComponent<SpriteRenderer>();
        heldRenderer.sortingOrder = 100;
        clubIdleSprite = ToSprite(clubIdleTexture);
        clubUpSprite = ToSprite(clubUpTexture);
        clubDownSprite = ToSprite(clubDownTexture);
        sodaIdleSprite = ToSprite(sodaIdleTexture);
        sodaFireSprite = ToSprite(sodaFireTexture);
        heldRenderer.sprite = null;
        heldRenderer.enabled = false;
    }

    private void SetMode(BottleMode requestedMode)
    {
        if (requestedMode == mode) return;
        mode = requestedMode;
        if (hasCola) SetIdleSprite();
        statusMessage = mode == BottleMode.Club ? "CLUB MODE" : "SODA MODE";
        feedbackUntil = Time.unscaledTime + 0.5f;
    }

    public void CollectCola()
    {
        if (hasCola) return;
        hasCola = true;
        if (pickupObject != null) Destroy(pickupObject);
        if (heldRenderer != null) heldRenderer.enabled = true;
        SetIdleSprite();
        statusMessage = "COLA PICKED UP";
        feedbackUntil = Time.unscaledTime + 1f;
        Debug.Log("Cola picked up. Press 1 for club, 2 for soda.");
    }

    private void SpawnPickup()
    {
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f) flatForward = Vector3.forward;
        flatForward.Normalize();

        Vector3 spawnPosition = transform.position + flatForward * pickupSpawnDistance;
        spawnPosition.y = transform.position.y;

        pickupObject = new GameObject("Cola Pickup (auto-spawned)");
        pickupObject.transform.position = spawnPosition;
        ColaPickup pickup = pickupObject.AddComponent<ColaPickup>();
        pickup.Configure(this, playerCamera, Resources.Load<Texture2D>("Weapons/Cola/yerde"));
    }

    private void SetIdleSprite()
    {
        if (heldRenderer == null) return;
        heldRenderer.sprite = mode == BottleMode.Club ? clubIdleSprite : sodaIdleSprite;
        heldRenderer.enabled = hasCola;
    }

    private void PlayClubAttack()
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(ClubAnimation());
    }

    private IEnumerator ClubAnimation()
    {
        if (heldRenderer != null) heldRenderer.sprite = clubUpSprite;
        yield return new WaitForSeconds(clubFrameDuration);
        if (heldRenderer != null) heldRenderer.sprite = clubDownSprite;
        yield return new WaitForSeconds(clubFrameDuration);
        if (mode == BottleMode.Club) SetIdleSprite();
        animationRoutine = null;
    }

    private void PlaySodaAttack()
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(SodaAnimation());
    }

    private IEnumerator SodaAnimation()
    {
        if (heldRenderer != null) heldRenderer.sprite = sodaFireSprite;
        yield return new WaitForSeconds(sodaAttackDuration);
        if (mode == BottleMode.Soda) SetIdleSprite();
        animationRoutine = null;
    }

    private void Attack(float range, float damage, string attackName)
    {
        lastAttackHit = false;
        statusMessage = attackName;
        feedbackUntil = Time.unscaledTime + 0.18f;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit[] hits = Physics.RaycastAll(ray, range, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;

                lastAttackHit = ApplyDamage(hit.collider, damage);
                if (lastAttackHit)
                {
                    hitUntil = Time.unscaledTime + 0.18f;
                    statusMessage = attackName + " - HIT!";
                }
                else
                    statusMessage = attackName + " - NO TARGET";
                break;
            }
        }

        Debug.Log("Cola " + attackName + (lastAttackHit ? ": HIT" : ": MISS / no damage receiver"));
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
            !hasCola ? "COLA: NOT FOUND" :
            mode == BottleMode.Club ? "COLA: CLUB  |  1: CLUB  |  2: SODA  |  LMB: SWING"
            : "COLA: SODA  |  1: CLUB  |  2: SODA  |  LMB: FIRE");
        if (hasCola && Time.unscaledTime < feedbackUntil)
            GUI.Label(new Rect(cx + 18f, cy + 12f, 260f, 24f), statusMessage);
        GUI.color = Color.white;
    }
}
