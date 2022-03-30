using Assets.Scripts.Synchrony;
using Synchrony;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WalkScript : MonoBehaviour
{
    static int turnLeft90Hash = Animator.StringToHash("Turn Left 90 1");

    private bool isTurnLeftPressed = false;
    private bool isIdlePressed = false;
    private bool isWalkPressed = false;
    private bool isTurningLeft = false;

    private GameObject laika;
    Animator animator = null;


    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        laika = GetLaika;
        animator = laika.GetComponent<Animator>();
        animator.applyRootMotion = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isIdlePressed)
        {
            animator.CrossFade(stateName: "Idle 1", normalizedTransitionDuration: 0.1f);
        }

        if (isTurnLeftPressed)
        {
            //animator.Play(stateName: "Turn Left 90 1");
            animator.CrossFade(stateName: "Turn Left 90 1", normalizedTransitionDuration: 0.1f);
            isTurningLeft = true;
            //Laika.transform.position = Laika.transform.position - Vector3.forward * 0.1f;
        }

        if (isWalkPressed)
        {
            animator.CrossFade(stateName: "Walk 1", normalizedTransitionDuration: 0.1f);

            //Laika.transform.position = Laika.transform.position - Vector3.forward * 0.1f;
        }

        // When Turn Left is completed, move back to idle and rotate the Avatar in idle state accordingly
        var state = animator.GetCurrentAnimatorStateInfo(layerIndex: 0);
        if (state.shortNameHash == turnLeft90Hash)
        {
            "Turning Left".Log();

            var isLeftTurnComplete = state.normalizedTime >= state.length && isTurningLeft;
            if (isLeftTurnComplete)
            {
                isTurningLeft = false;

                "LeftTurnComplete".Log();

                // workaround for left turn rotation not being applied to gameobject
                var rootMotion = laika.transform.Find("RootMotion");
                var rootJoint = rootMotion.transform.Find("Tatra_ROOTSHJnt");

                laika.transform.rotation = laika.transform.rotation * Quaternion.Euler(0, -90, 0);  //rootJoint.transform.rotation;
                rootJoint.transform.rotation = Quaternion.Euler(0, 0, 0); //Quaternion.identity;
                //laika.transform.rotation = laika.transform.rotation * Quaternion.AngleAxis(angle: -90, axis: Vector3.up);

                //TODO: set a CoRoutine to move to IdleState on the next update cycle, this is needed so ensure the above is applied
                animator.CrossFade(stateName: "Idle", normalizedTransitionDuration: 0.05f);

                //animator.Play(stateName: "Walk");
            }
        }
    }

    void OnAnimatorMove()
    {
        //$"Body Rotation: {animator.bodyRotation}".Log();

        transform.position = animator.rootPosition;
        transform.rotation = animator.rootRotation;

        //animator.ApplyBuiltinRootMotion();
    }

    GameObject GetLaika
    {
        get
        {
            return GameObject.Find("Laika");
        }
    }

    void OnGUI()
    {
        GUI.enabled = true;

        isWalkPressed = GUI.Button(
           position: new Rect(x: 20, y: 0, width: 300, height: 60),
           text: "Walk forward"
           /* ,style: GuiStyles.Button*/);

        isTurnLeftPressed = GUI.Button(
            position: new Rect(x: 20, y: 100, width: 300, height: 60),
            text: "Turn Left"
            /* ,style: GuiStyles.Button*/);

        isIdlePressed = GUI.Button(
            position: new Rect(x: 20, y: 200, width: 300, height: 60),
            text: "Idle"
            /* ,style: GuiStyles.Button*/);
    }
}
