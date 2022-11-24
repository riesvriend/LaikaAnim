using Synchrony;
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
    private int animalLayer;
    private TimeSpan restartTimeSpan;
    private DateTime? playingStartedUtc;

    private void Start()
    {
        if (combingVibrationAudio != null)
            combingVibrationHapticsClip = new OVRHapticsClip(combingVibrationAudio);

        audioSource = gameObject.AddComponent<AudioSource>();

        animalLayer = LayerMask.NameToLayer("Animal");
        restartTimeSpan = TimeSpan.FromSeconds(restartLoopAfterSec);
    }

    private void Update()
    {
        if (playingStartedUtc.HasValue)
        {
            var playingTime = DateTime.UtcNow - playingStartedUtc.Value;
            if (playingTime > restartTimeSpan)
                // The haptics continues to play for about 2 seconds according to docs
                // so we periodically need to restart it to keep it going
                StartPlaying();
        }
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

        playingStartedUtc = DateTime.UtcNow;
    }

    public void OnSkinTouchExit()
    {
        if (combingVibrationHapticsClip == null)
            return;

        playingStartedUtc = null;
        OVRHaptics.RightChannel.Clear();
        audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == animalLayer)
        {
            Debug.Log(
                $"OnTriggerEnter in {gameObject.FullName()} with {other.gameObject.FullName()}"
            );

            OnSkinTouchEnter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == animalLayer)
        {
            Debug.Log(
                $"OnTriggerExit in {gameObject.FullName()} with {other.gameObject.FullName()}"
            );

            OnSkinTouchExit();
        }
    }
}
