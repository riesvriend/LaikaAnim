using MalbersAnimations.Controller;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Synchrony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class GrabbableObjectHaptics : MonoBehaviour
{
    public class StrokeEvent
    {
        public MAnimal Animal;
    }

    public class GrabItemEvent
    {
        public GameObject GrabbableObject;
    }

    [SerializeField]
    private Grabbable _grabbable;

    //public AudioClip combingVibrationAudio;

    // Plays a brushing sound when brushing
    public AudioSource audioSource;
    public float audioFadeoutTimeSec = 1.0f;
    public string AnimalLayerToCollideWith = "Animal";

    public UnityEvent<StrokeEvent> OnStrokingStarted = new();
    public UnityEvent<StrokeEvent> OnStrokingStopped = new();
    public UnityEvent<GrabItemEvent> OnItemGrabbed = new();
    public UnityEvent OnItemGrabReleased = new();

    private int animalLayer;
    private TimeSpan restartTimeSpan;
    private float restartLoopAfterSec = 1.5f;
    private DateTime? hapticsStartedUtc;

    private DateTime? audioFadeOutEndUtc = null;

    protected bool _started = false;

    private MAnimal activeAnimal = null;
    private OVRInput.Controller activeController = OVRInput.Controller.None;

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
            // BUG: HANGs on on converting the audio clip, the OVRHaptics.Config.SampleRateHz is 0!
            // https://forums.oculusvr.com/t5/Unity-VR-Development/the-quot-OVRHapticsClip-quot-can-t-work-help-me-to-write-some/td-p/493037
            //if (combingVibrationAudio != null)
            //    combingVibrationHapticsClip = new OVRHapticsClip(combingVibrationAudio);
            animalLayer = LayerMask.NameToLayer(AnimalLayerToCollideWith);
            restartTimeSpan = TimeSpan.FromSeconds(restartLoopAfterSec);
            _grabbable.WhenPointerEventRaised += HandlePointerEventRaised;
        }
    }

    protected virtual void OnDisable()
    {
        if (_started)
            _grabbable.WhenPointerEventRaised -= HandlePointerEventRaised;
    }

    private void HandlePointerEventRaised(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                // Set the active controller, by looking at the interactor that is grabbing the comb
                var interactor = evt.Data as MonoBehaviour; // this is the <Controller/Hand>GrabInteractor
                if (interactor == null)
                    SetActiveController(OVRInput.Controller.None);
                else
                {
                    var controllerRef = interactor.GetComponent<ControllerRef>();
                    if (controllerRef != null)
                        SetActiveController(
                            controllerRef.Handedness == Handedness.Right
                                ? OVRInput.Controller.RTouch
                                : OVRInput.Controller.LTouch
                        );
                    else
                        // Presume its hands free mode, which hand does not matter as
                        // we can't use the haptics anyway but we want keep track of the brush count
                        SetActiveController(OVRInput.Controller.RHand);
                }

                break;

            case PointerEventType.Unselect:
                OnCombingStopped();
                SetActiveController(OVRInput.Controller.None);
                break;
        }
    }

    void SetActiveController(OVRInput.Controller controller)
    {
        if (activeController == controller)
            return;

        activeController = controller;
        if (activeController == OVRInput.Controller.None)
            OnItemGrabReleased.Invoke();
        else
            OnItemGrabbed.Invoke(new GrabItemEvent { GrabbableObject = _grabbable.gameObject });
    }

    private void Update()
    {
        RestartHapticsAfterAutomaticTimeout();

        StopAudioAfterFadeOutTimeElapsed();

        // The haptics continues to play for about 2 seconds according to docs
        // so we periodically need to restart it to keep it going
        void RestartHapticsAfterAutomaticTimeout()
        {
            if (hapticsStartedUtc.HasValue)
            {
                var playingTime = DateTime.UtcNow - hapticsStartedUtc.Value;
                if (playingTime > restartTimeSpan)
                    StartHaptics();
            }
        }

        // The fade-out delay allows the the eating/combing sound to be completed and not be stop/restarting all the time
        // if the user grab touches the target frequently
        void StopAudioAfterFadeOutTimeElapsed()
        {
            if (audioFadeOutEndUtc != null && DateTime.UtcNow > audioFadeOutEndUtc)
            {
                audioFadeOutEndUtc = null;
                audioSource.Stop();
            }
        }
    }

    private void OnSkinTouchEnter(MAnimal animal)
    {
        OnCombingStarted(animal);
    }

    private void OnCombingStarted(MAnimal animal)
    {
        if (activeController == OVRInput.Controller.None)
            // If the comb is dropped on a animal while not grabbing it
            // the controller should not vibrate
            return;

        activeAnimal = animal;

        StartHaptics();

        // Increase stroking counter in the game and arrange awards
        OnStrokingStarted.Invoke(new StrokeEvent { Animal = activeAnimal });
    }

    private void OnSkinTouchExit()
    {
        OnCombingStopped();
    }

    private void OnCombingStopped()
    {
        var animal = activeAnimal;
        activeAnimal = null;
        StopHaptics();
        OnStrokingStopped.Invoke(new StrokeEvent { Animal = animal });

        void StopHaptics()
        {
            hapticsStartedUtc = null;
            SetVibrationOnActiveController(frequency: 0f, amplitude: 0f);
            audioFadeOutEndUtc = DateTime.UtcNow + TimeSpan.FromSeconds(audioFadeoutTimeSec);
            //audioSource.Stop();
        }
    }

    private void SetVibrationOnActiveController(float frequency, float amplitude)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, activeController);
    }

    private void StartHaptics()
    {
        if (audioFadeOutEndUtc.HasValue)
            // Cancel the fade out
            audioFadeOutEndUtc = null;
        else
            audioSource.Play();
        SetVibrationOnActiveController(frequency: .3f, amplitude: .7f);
        hapticsStartedUtc = DateTime.UtcNow;
    }

    MAnimal MalbersAnimalOf(Collider other)
    {
        if (other.gameObject.layer == animalLayer)
        {
            Debug.Log(
                $"OnTriggerEnter in {gameObject.FullName()} with {other.gameObject.FullName()}"
            );

            var malbersAnimal = other.gameObject.GetComponentInParent<MAnimal>();

            if (malbersAnimal != null)
                return malbersAnimal;
        }
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        var malbersAnimal = MalbersAnimalOf(other);

        if (malbersAnimal != null)
            OnSkinTouchEnter(malbersAnimal);
    }

    private void OnTriggerExit(Collider other)
    {
        var malbersAnimal = MalbersAnimalOf(other);

        if (malbersAnimal != null)
            OnSkinTouchExit();
    }
}
