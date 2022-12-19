using Oculus.Interaction;
using Oculus.Interaction.Input;
using Synchrony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class CombTouchHaptics : MonoBehaviour
{
    public class StrokeEvent
    {
        public GameObject Animal;
    }

    [SerializeField]
    private Grabbable _grabbable;
    public float restartLoopAfterSec = 1.5f;
    public UnityEvent<StrokeEvent> OnStrokingStarted = new();
    public UnityEvent<StrokeEvent> OnStrokingStopped = new();

    private int animalLayer;
    private TimeSpan restartTimeSpan;
    private DateTime? playingStartedUtc;

    protected bool _started = false;

    private OVRInput.Controller activeController = OVRInput.Controller.None;

    private void Awake()
    {
        // BUG: HANGs on on converting the audio clip, the OVRHaptics.Config.SampleRateHz is 0!
        // https://forums.oculusvr.com/t5/Unity-VR-Development/the-quot-OVRHapticsClip-quot-can-t-work-help-me-to-write-some/td-p/493037
        //if (combingVibrationAudio != null)
        //    combingVibrationHapticsClip = new OVRHapticsClip(combingVibrationAudio);
        //audioSource = gameObject.AddComponent<AudioSource>();
        animalLayer = LayerMask.NameToLayer("Animal");
        restartTimeSpan = TimeSpan.FromSeconds(restartLoopAfterSec);
    }

    protected virtual void Start()
    {
        this.BeginStart(ref _started);
        Assert.IsNotNull(_grabbable);
        this.EndStart(ref _started);
    }

    protected virtual void OnEnable()
    {
        if (_started)
        {
            _grabbable.WhenPointerEventRaised += HandlePointerEventRaised;
        }
    }

    protected virtual void OnDisable()
    {
        if (_started)
        {
            _grabbable.WhenPointerEventRaised -= HandlePointerEventRaised;
        }
    }

    private void HandlePointerEventRaised(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                // Set the active controller, by looking at the interactor that is grabbing the comb
                activeController = OVRInput.Controller.RTouch;

                var interactor = evt.Data as MonoBehaviour; // this is the ControllerGrabInteractor
                if (interactor != null)
                {
                    var controllerRef = interactor.GetComponent<ControllerRef>();
                    if (controllerRef != null)
                        activeController =
                            controllerRef.Handedness == Handedness.Right
                                ? OVRInput.Controller.RTouch
                                : OVRInput.Controller.LTouch;
                }

                break;
        }
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

    private void OnSkinTouchEnter(GameObject animalBodyPart)
    {
        StartPlaying();
        OnStrokingStarted.Invoke(
            new StrokeEvent { Animal = animalBodyPart.transform.root.gameObject }
        );
    }

    private void OnSkinTouchExit(GameObject animalBodyPart)
    {
        StopPlaying();
        OnStrokingStopped.Invoke(
            new StrokeEvent { Animal = animalBodyPart.transform.root.gameObject }
        );
    }

    private void StartPlaying()
    {
        OVRInput.SetControllerVibration(frequency: .3f, amplitude: .7f, activeController);

        playingStartedUtc = DateTime.UtcNow;
    }

    private void StopPlaying()
    {
        playingStartedUtc = null;
        OVRHaptics.RightChannel.Clear();
        OVRInput.SetControllerVibration(frequency: 0, amplitude: 0, activeController);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == animalLayer)
        {
            Debug.Log(
                $"OnTriggerEnter in {gameObject.FullName()} with {other.gameObject.FullName()}"
            );

            OnSkinTouchEnter(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == animalLayer)
        {
            Debug.Log(
                $"OnTriggerExit in {gameObject.FullName()} with {other.gameObject.FullName()}"
            );

            OnSkinTouchExit(other.gameObject);
        }
    }
}
