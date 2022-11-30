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

    private void Awake()
    {
        // BUG: HANGs on on converting the audio clip, the OVRHaptics.Config.SampleRateHz is 0!
        // https://forums.oculusvr.com/t5/Unity-VR-Development/the-quot-OVRHapticsClip-quot-can-t-work-help-me-to-write-some/td-p/493037
        //if (combingVibrationAudio != null)
        //    combingVibrationHapticsClip = new OVRHapticsClip(combingVibrationAudio);

        audioSource = gameObject.AddComponent<AudioSource>();

        animalLayer = LayerMask.NameToLayer("Animal");
        restartTimeSpan = TimeSpan.FromSeconds(restartLoopAfterSec);
    }

    private void Start() { }

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
        StartPlaying();
    }

    private void StartPlaying()
    {
        if (combingVibrationHapticsClip != null)
        {
            OVRHaptics.RightChannel.Preempt(combingVibrationHapticsClip);
        }
        else
        {
            OVRInput.SetControllerVibration(
                frequency: .3f,
                amplitude: .5f,
                OVRInput.Controller.RTouch
            );
        }
        audioSource.clip = combingVibrationAudio;
        audioSource.Play();

        playingStartedUtc = DateTime.UtcNow;
    }

    private void StopPlaying()
    {
        playingStartedUtc = null;
        OVRHaptics.RightChannel.Clear();
        OVRInput.SetControllerVibration(frequency: 0, amplitude: 0, OVRInput.Controller.RTouch);
        audioSource.Stop();
    }

    public void OnSkinTouchExit()
    {
        StopPlaying();
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
