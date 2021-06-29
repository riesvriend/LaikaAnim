using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaikaMovement : MonoBehaviour
{
    Animator animator;
    PlayerInput input;
    int isRestingHash;
    bool isUpKeyPressed;
    bool isDownKeyPressed;

    private void OnEnable()
    {
        input = new PlayerInput();
        input.DogControls.Up.performed += Up_performed;
        input.DogControls.Down.performed += Down_performed;
        input.DogControls.VerticalAcceleleration.performed += VerticalAcceleleration_performed;
        input.DogControls.Enable();
        if (HasAccelerometer())
            InputSystem.EnableDevice(Accelerometer.current);
    }

    private void OnDestroy()
    {
        input.DogControls.Up.performed -= Up_performed;
        input.DogControls.Down.performed -= Down_performed;
        input.DogControls.VerticalAcceleleration.performed -= VerticalAcceleleration_performed;
        input = null;
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
        //Debug.Log(ctx.ReadValueAsObject());
        isUpKeyPressed = ctx.ReadValue<float>() == 1;
    }

    private void VerticalAcceleleration_performed(InputAction.CallbackContext ctx)
    {
        var acceleration = ctx.ReadValue<float>();
        isUpKeyPressed = acceleration > 0;
        isDownKeyPressed = acceleration < 0 && !isUpKeyPressed;
    }

    private static bool HasAccelerometer()
    {
        return InputSystem.devices.Any(d => d.valueType == typeof(Accelerometer));
    }

    private void OnDisable()
    {
        input.DogControls.Disable();
        if (HasAccelerometer())
            InputSystem.DisableDevice(Accelerometer.current);
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
            var isAnimationInRestState = animator.GetBool("isResting");
            if (isAnimationInRestState)
                animator.SetBool("isResting", false);
        }

        if (isDownKeyPressed)
        {
            isDownKeyPressed = false;
            var isAnimationInRestState = animator.GetBool("isResting");
            if (!isAnimationInRestState)
                animator.SetBool("isResting", true);
        }
    }
}

