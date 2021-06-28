using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaikaMovement : MonoBehaviour
{
    Animator animator;

    int isRestingHash;

    PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
        input.DogControls.Up.performed += Up_performed;
    }

    private void OnEnable()
    {
        input.DogControls.Enable();
    }

    private void OnDisable()
    {
        input.DogControls.Disable();
    }

    private void Up_performed(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.ReadValueAsObject());
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
        
    }
}
