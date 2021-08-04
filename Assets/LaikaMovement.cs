using Synchrony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaikaMovement : MonoBehaviour
{
    public class AccelerationSample
    {
        public float acceleration;
        public DateTime dateTimeUtc;
    }

    Animator animator;
    PlayerInput input;
    int isRestingHash;
    bool isUpKeyPressed;
    bool isDownKeyPressed;

    List<AccelerationSample> accelerationSamples = new List<AccelerationSample>();


    private void Awake()
    {
        input = new PlayerInput();
        input.DogControls.Up.performed += Up_performed;
        input.DogControls.Down.performed += Down_performed;
        // LinearAccelaration Sensor does not exist on iPad, so we use AcceleroMeter
        input.DogControls.VerticalAcceleleration.performed += VerticalAcceleleration_performed;
        input.DogControls.Acceleration.performed += Acceleration_performed;
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.DogControls.Up.performed -= Up_performed;
            input.DogControls.Down.performed -= Down_performed;
            input.DogControls.VerticalAcceleleration.performed -= VerticalAcceleleration_performed;
            input.DogControls.Acceleration.performed -= Acceleration_performed;
            input = null;
        }
    }

    private void OnEnable()
    {
        input?.DogControls.Enable();
        if (HasAccelerometer())
            InputSystem.EnableDevice(Accelerometer.current);
    }

    private void OnDisable()
    {
        input?.DogControls.Disable();
        if (HasAccelerometer())
            InputSystem.DisableDevice(Accelerometer.current);
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        isRestingHash = Animator.StringToHash("isResting");
    }

    private void Down_performed(InputAction.CallbackContext ctx)
    {
        isDownKeyPressed = ctx.ReadValue<float>() == 1; // 0 on key release, 1 on key press
    }
    private void Up_performed(InputAction.CallbackContext ctx)
    {
        isUpKeyPressed = ctx.ReadValue<float>() == 1;
    }

    private void Acceleration_performed(InputAction.CallbackContext ctx)
    {
        var acceleration = ctx.ReadValue<float>();
        $"Accelleration: {acceleration}. duration: {ctx.duration}".Log();

        // Calculate increase in acceleration over the last second. If > 2/ms2 then the user 
        // pushed the device up or down
    }

    private void VerticalAcceleleration_performed(InputAction.CallbackContext ctx)
    {
        var acceleration = ctx.ReadValue<float>();
        $"Linear Accelleration: {acceleration}. duration: {ctx.duration}".Log();
        isUpKeyPressed = acceleration > 2; // meter per second
        isDownKeyPressed = acceleration < 2 && !isUpKeyPressed;
    }

    private static bool HasAccelerometer()
    {
        var hasAccelerometer = UnityEngine.InputSystem.Accelerometer.current != null;
        $"Accelerometer: {hasAccelerometer}".Log();
        return hasAccelerometer;
    }

    private static bool HasLinearAccelerationSensor()
    {
        var hasLinearSensor = UnityEngine.InputSystem.LinearAccelerationSensor.current != null;
        $"HasLinearAccelerationSensor: {hasLinearSensor}".Log();

        var hasAccelerometer = UnityEngine.InputSystem.Accelerometer.current != null;
        $"Accelerometer: {hasAccelerometer}".Log();

        foreach (var d in InputSystem.devices)
            $"Device: {d.GetType().FullName}".Log();
        return InputSystem.devices.Any(d => d.GetType().IsClassOrSubclass<LinearAccelerationSensor>());
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (isUpKeyPressed)
        {
            isUpKeyPressed = false;
            var isAnimationInRestState = animator.GetBool(isRestingHash);
            if (isAnimationInRestState)
                animator.SetBool(isRestingHash, false);
        }

        if (isDownKeyPressed)
        {
            isDownKeyPressed = false;
            var isAnimationInRestState = animator.GetBool(isRestingHash);
            if (!isAnimationInRestState)
                animator.SetBool(isRestingHash, true);
        }
    }
}

