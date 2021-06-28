using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaikaMovement : MonoBehaviour
{
    Animator animator;
    PlayerInput input;
    int isRestingHash;
    bool isUpKeyPressed;
    bool isDownKeyPressed;

    private void Awake()
    {
        input = new PlayerInput();
        input.DogControls.Up.performed += Up_performed;
        input.DogControls.Down.performed += Down_performed;
    }

    private void OnDestroy()
    {
        input.DogControls.Up.performed -= Up_performed;
        input.DogControls.Down.performed -=  Down_performed;
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

    private void OnEnable()
    {
        input.DogControls.Enable();
    }

    private void OnDisable()
    {
        input.DogControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        isRestingHash = Animator.StringToHash("isResting");
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
