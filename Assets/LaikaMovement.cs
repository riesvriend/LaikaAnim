using Synchrony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))] // For OnMouseDown
public class LaikaMovement : MonoBehaviour
{
    Animator animator;
    PlayerInput input;
    VerticalAccelerationSensor verticalAccelerationSensor;
    VoiceController voiceController;
    App app;

    bool isConstructed = false;
    int isRestingHash;
    bool isUpKeyPressed;
    bool isDownKeyPressed;

    // Event order https://docs.unity3d.com/Manual/ExecutionOrder.html
    //
    // OnEnable
    // OnStart
    // OnApplicationPause -> OnDisable (called when lost focus)
    // OnEnable 
    // OnDisable
    // OnDestroy

    // Called on first Enable, this makes sure we can find references to global game objects in the DontDestroyOnLoad hidden scene
    // which may be still pending if we would use Unity's Awake message instead
    private void Constructor()
    {
        if (!isConstructed)
        {
            isConstructed = true;

            $"LaikaMovement Constructor".Log();

            input = new PlayerInput();
            input.DogControls.Up.performed += Up_performed;
            input.DogControls.Down.performed += Down_performed;
            // LinearAccelaration Sensor does not exist on iPad, so we use AcceleroMeter
            //input.DogControls.VerticalAcceleleration.performed += VerticalAcceleleration_performed;
            //input.DogControls.Acceleration.performed += Acceleration_performed;

            verticalAccelerationSensor = new VerticalAccelerationSensor();

            var app = App.GetApp();
            voiceController = app.ChildComponent<VoiceController>();
            voiceController.PartialSpeechResultEvent.AddListener(OnPartialSpeechResult);

            animator = GetComponent<Animator>();
            isRestingHash = Animator.StringToHash("isResting");
        }
    }

    private void OnEnable()
    {
        $"LaikaMovement OnEnable".Log();
        Constructor();

        input.DogControls.Enable();
        verticalAccelerationSensor.OnEnable();
    }


    private void OnDisable()
    {
        $"LaikaMovement OnDisable".Log();
        input.DogControls.Disable();
        verticalAccelerationSensor.OnDisable();
        voiceController.StopListening();
    }


    private void OnDestroy()
    {
        $"LaikaMovement OnDestroy".Log();

        input.DogControls.Up.performed -= Up_performed;
        input.DogControls.Down.performed -= Down_performed;
        //input.DogControls.VerticalAcceleleration.performed -= VerticalAcceleleration_performed;
        //input.DogControls.Acceleration.performed -= Acceleration_performed;
        input = null;

        verticalAccelerationSensor.OnDestroy();
        verticalAccelerationSensor = null;

        voiceController.StopListening();
        voiceController.PartialSpeechResultEvent.RemoveAllListeners();
        voiceController = null;
    }

    /// <summary>
    /// When tablet is shut down or tabbed away to other app
    /// https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationPause.html 
    /// </summary>
    /// <param name="pause"></param>
    private void OnApplicationPause(bool pause)
    {
        $"LaikaMovement OnApplicationPause pause: {pause}".Log();

        if (!pause)
        {
            OnEnable();
        }
        else // pause
        {
            OnDisable();
        }
    }


    private void Down_performed(InputAction.CallbackContext ctx)
    {
        isDownKeyPressed = ctx.ReadValue<float>() == 1; // 0 on key release, 1 on key press
    }
    private void Up_performed(InputAction.CallbackContext ctx)
    {
        isUpKeyPressed = ctx.ReadValue<float>() == 1;
    }


    // Update is called once per frame
    void Update()
    {
        if (verticalAccelerationSensor == null)
        {
            "LaikaMovement verticalAccelerationSensor is null after leaving Play Mode".Log();
            return;
        }

        verticalAccelerationSensor.TakeSample();

        if (!isUpKeyPressed)
            isUpKeyPressed = verticalAccelerationSensor.IsPushedUp();
        if (!isDownKeyPressed)
            isDownKeyPressed = verticalAccelerationSensor.IsPushedDown();

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (isUpKeyPressed)
        {
            isUpKeyPressed = false;
            var isAnimationInRestState = animator.GetBool(isRestingHash);
            if (isAnimationInRestState)
            {
                voiceController.StartSpeaking("Going up");
                voiceController.StartListening();

                animator.SetBool(isRestingHash, false);
            }
        }

        if (isDownKeyPressed)
        {
            isDownKeyPressed = false;
            //voiceController.StopListening();

            var isAnimationInRestState = animator.GetBool(isRestingHash);
            if (!isAnimationInRestState)
                animator.SetBool(isRestingHash, true);
        }
    }

    private List<string> upCommands = new List<string> { "laika", "sta", "kom", "hier", "stand", "up", "come", "here" };
    private List<string> downCommands = new List<string> { "af", "lig", "zit", "down", "sit", "lie" };
    private void OnPartialSpeechResult(string result)
    {
        var wordsSpoken = result.ToLowerInvariant().Split(' ').ToList();
        if (upCommands.Any(c => wordsSpoken.Contains(c)))
            this.isUpKeyPressed = true;
        else if (downCommands.Any(c => wordsSpoken.Contains(c)))
            this.isDownKeyPressed = true;
    }


    //private void Acceleration_performed(InputAction.CallbackContext ctx)
    //{
    //    var acceleration = ctx.ReadValue<float>();
    //    //$"Accelleration: {acceleration}. duration: {ctx.duration}".Log();

    //    // Calculate increase in acceleration over the last second. If > 2/ms2 then the user 
    //    // pushed the device up or down
    //}

    //private void VerticalAcceleleration_performed(InputAction.CallbackContext ctx)
    //{
    //    var acceleration = ctx.ReadValue<float>();
    //    $"Linear Accelleration: {acceleration}. duration: {ctx.duration}".Log();
    //    isUpKeyPressed = acceleration > 2; // meter per second
    //    isDownKeyPressed = acceleration < 2 && !isUpKeyPressed;
    //}

    //private static bool HasLinearAccelerationSensor()
    //{
    //    var hasLinearSensor = UnityEngine.InputSystem.LinearAccelerationSensor.current != null;
    //    $"HasLinearAccelerationSensor: {hasLinearSensor}".Log(); // False on iPad

    //    var hasAccelerometer = UnityEngine.InputSystem.Accelerometer.current != null;
    //    $"Has Accelerometer: {hasAccelerometer}".Log(); // True on iPad and Samsung Tab

    //    foreach (var d in InputSystem.devices)
    //        $"Device: {d.GetType().FullName}".Log();
    //    return InputSystem.devices.Any(d => d.GetType().IsClassOrSubclass<LinearAccelerationSensor>());
    //}

}

