using System.Collections;
using UnityEngine;

/// <summary>
/// Frame-based sprite animation for a 2.5D enemy.
/// Assign your own Krita-exported sprites in the Inspector; no placeholder art is created.
/// </summary>
[DisallowMultipleComponent]
public class EnemySpriteAnimation : MonoBehaviour
{
    [Header("Sprite setup")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] shootFrames;

    [Header("Frame rates")]
    [Min(1f)] [SerializeField] private float idleFramesPerSecond = 6f;
    [Min(1f)] [SerializeField] private float walkFramesPerSecond = 10f;
    [Min(1f)] [SerializeField] private float shootFramesPerSecond = 12f;
    [SerializeField] private bool loopIdle = true;
    [SerializeField] private bool loopWalk = true;

    private Coroutine playback;
    private State currentState = State.None;

    private enum State { None, Idle, Walk, Shoot }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        PlayIdle();
    }

    public void PlayIdle() => PlayLoop(State.Idle, idleFrames, idleFramesPerSecond, loopIdle);
    public void PlayWalk() => PlayLoop(State.Walk, walkFrames, walkFramesPerSecond, loopWalk);

    /// <summary>Plays the shooting frames once, then returns to idle.</summary>
    public void PlayShoot()
    {
        if (shootFrames == null || shootFrames.Length == 0)
        {
            PlayIdle();
            return;
        }

        StartPlayback(State.Shoot, shootFrames, shootFramesPerSecond, false, true);
    }

    private void PlayLoop(State state, Sprite[] frames, float fps, bool loop)
    {
        if (frames == null || frames.Length == 0)
            return;
        StartPlayback(state, frames, fps, loop, false);
    }

    private void StartPlayback(State state, Sprite[] frames, float fps, bool loop, bool returnToIdle)
    {
        if (targetRenderer == null || frames == null || frames.Length == 0)
            return;
        if (currentState == state && playback != null)
            return;

        if (playback != null)
            StopCoroutine(playback);

        currentState = state;
        playback = StartCoroutine(PlayFrames(state, frames, fps, loop, returnToIdle));
    }

    private IEnumerator PlayFrames(State state, Sprite[] frames, float fps, bool loop, bool returnToIdle)
    {
        float delay = 1f / Mathf.Max(1f, fps);
        do
        {
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] != null)
                    targetRenderer.sprite = frames[i];
                yield return new WaitForSeconds(delay);
            }
        }
        while (loop);

        playback = null;
        if (returnToIdle)
            PlayIdle();
        else if (currentState != state)
            yield break;
    }

    private void OnDisable()
    {
        if (playback != null)
        {
            StopCoroutine(playback);
            playback = null;
        }
        currentState = State.None;
    }
}