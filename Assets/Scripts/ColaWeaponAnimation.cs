using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a single-shot sprite animation for a Cola weapon.
/// Assign the idle sprite and the ordered firing frames in the Inspector.
/// This component does not fire repeatedly while the mouse button is held.
/// </summary>
[DisallowMultipleComponent]
public class ColaWeaponAnimation : MonoBehaviour
{
    [Header("Sprite setup")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] fireFrames;

    [Header("Animation")]
    [Min(1f)]
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private bool restoreIdleAfterAnimation = true;

    private Coroutine playback;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null && idleSprite != null)
            targetRenderer.sprite = idleSprite;
    }

    /// <summary>Call once per successful shot.</summary>
    public void PlayFireAnimation()
    {
        if (targetRenderer == null || fireFrames == null || fireFrames.Length == 0)
            return;

        if (playback != null)
            StopCoroutine(playback);

        playback = StartCoroutine(PlayFrames());
    }

    private IEnumerator PlayFrames()
    {
        float frameDelay = 1f / Mathf.Max(1f, framesPerSecond);

        for (int i = 0; i < fireFrames.Length; i++)
        {
            if (fireFrames[i] != null)
                targetRenderer.sprite = fireFrames[i];

            yield return new WaitForSeconds(frameDelay);
        }

        if (restoreIdleAfterAnimation && idleSprite != null)
            targetRenderer.sprite = idleSprite;

        playback = null;
    }

    private void OnDisable()
    {
        if (playback != null)
        {
            StopCoroutine(playback);
            playback = null;
        }

        if (targetRenderer != null && idleSprite != null)
            targetRenderer.sprite = idleSprite;
    }
}