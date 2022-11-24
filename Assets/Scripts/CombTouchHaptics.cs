using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombTouchHaptics : MonoBehaviour
{
    public AudioClip combingVibrationAudio;
    public float restartLoopAfterSec = 1.5f;

    private OVRHapticsClip combingVibrationHapticsClip;
    private AudioSource audioSource;

    private DateTime startedUtc;

    private void Start()
    {
        if (combingVibrationAudio != null)
            combingVibrationHapticsClip = new OVRHapticsClip(combingVibrationAudio);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (DateTime.UtcNow - startedUtc > TimeSpan.FromSeconds(restartLoopAfterSec))
            StartPlaying();
    }

    public void OnSkinTouchEnter()
    {
        if (combingVibrationHapticsClip == null)
            return;

        StartPlaying();
    }

    private void StartPlaying()
    {
        OVRHaptics.RightChannel.Preempt(combingVibrationHapticsClip);
        audioSource.clip = combingVibrationAudio;
        audioSource.Play();

        startedUtc = DateTime.UtcNow;
    }

    public void OnSkinTouchExit()
    {
        if (combingVibrationHapticsClip == null)
            return;

        OVRHaptics.RightChannel.Clear();
        audioSource.Stop();
    }
}
