using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Synchrony;
using static Synchrony.SynchronyUtils;

/// <summary>
/// Allows moving the AR Camera while in Play mode in the Editor
/// </summary>
public class ARCameraMover : MonoBehaviour
{
    [SerializeField] float mouseScrollScale = 5f;
    [SerializeField] float mouseMoveScale = 5f;

    private Camera arCamera;

    // Start is called before the first frame update
    void Start()
    {
        arCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsPlayingInEditor())
            return;

        MoveCamera();
        RotateCamera();
    }

    /// <summary>
    /// Rotate the camera with the mouse plus ALT or OPTION key 
    /// </summary>
    private void RotateCamera()
    {
        var allowRotate = IsAltOrOptionPressed();
        if (!allowRotate)
            return;

        var xRotationDelta = Input.mouseScrollDelta.y;
        var yRotationDelta = Input.GetAxis("Mouse X");
        var zRotationDelta = Input.GetAxis("Mouse Y");

        arCamera.transform.Rotate(RotationDegrees(xRotationDelta), RotationDegrees(yRotationDelta), RotationDegrees(zRotationDelta));

        float RotationDegrees(float rotationDelta)
        {
            return rotationDelta * mouseMoveScale;
        }
    }
    

    // Move left/right & up/down by moving mouse (with mouse down, no ALT key)
    private void MoveCamera()
    {
        var allowMove = (Input.GetMouseButton(0) && !IsAltOrOptionPressed()) || IsMouseWheelTurned();
        if (!allowMove)
            return;

        // Left/Right and Up/Down
        var leftRightDelta = Input.GetAxis("Mouse X");
        var upDownDelta = Input.GetAxis("Mouse Y");

        $"leftRightDelta: {leftRightDelta}".Log();

        arCamera.transform.position += new Vector3(x: ScaledMouseDelta(leftRightDelta), y: ScaledMouseDelta(upDownDelta), z: 0);

        float ScaledMouseDelta(float mouseDelta)
        {
            return mouseDelta * mouseMoveScale * Time.deltaTime;
        }

        // Move camera forward/backward by scrolling mouse wheel
        var mouseScrollDeltaVector = Input.mouseScrollDelta; // mouseScrollDelta = (-)1*Time.deltaTime 
        var forwardDelta = mouseScrollDeltaVector.y;
        if (forwardDelta != 0)
        {
            var zOffset = mouseScrollScale * forwardDelta;
            arCamera.transform.position += new Vector3(x: 0, y: 0, z: zOffset);
        }
    }
}
