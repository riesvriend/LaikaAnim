using Synchrony;
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

    private void Awake()
    {
        input = new PlayerInput();
        input.DogControls.Up.performed += Up_performed;
        input.DogControls.Down.performed += Down_performed;
        input.DogControls.VerticalAcceleleration.performed += VerticalAcceleleration_performed;
    }

    private void OnDestroy()
    {
        input.DogControls.Up.performed -= Up_performed;
        input.DogControls.Down.performed -= Down_performed;
        input.DogControls.VerticalAcceleleration.performed -= VerticalAcceleleration_performed;
        input = null;
    }

    private void OnEnable()
    {
        input.DogControls.Enable();
        if (HasLinearAccelerationSensor())
            InputSystem.EnableDevice(LinearAccelerationSensor.current);
    }

    private void OnDisable()
    {
        input.DogControls.Disable();
        if (HasLinearAccelerationSensor())
            InputSystem.DisableDevice(LinearAccelerationSensor.current);
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

    private void VerticalAcceleleration_performed(InputAction.CallbackContext ctx)
    {
        var acceleration = ctx.ReadValue<float>();
        $"Accelleration: {acceleration}. duration: {ctx.duration}".Log();
        isUpKeyPressed = acceleration > 2; // meter per second
        isDownKeyPressed = acceleration < 2 && !isUpKeyPressed;
    }

    private static bool HasLinearAccelerationSensor()
    {
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

