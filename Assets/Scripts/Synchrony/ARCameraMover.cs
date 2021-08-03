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
/// Mouse: 
///  --left button down & move == move camera left right (X) and up down (Y)
///  --scroll is move forward & backward (Z)
/// </summary>
public class ARCameraMover : MonoBehaviour
{
    [SerializeField] float mouseScrollMetersPerUpdate = 0.5f;
    [SerializeField] float arrowKeyRotationDegreesPerSecond = 10f;
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
    /// Its not super intuitive, it needs a GUI rotation control similar to Unity Editor
    /// Similar to an aircraft, we want the camera to:
    /// - Right wing up/down to rotate X around Y (left/right)
    /// - Tail vertical left/right to rotate Y around (control left/right)
    /// - Tail wing up/down to rotate Z around X (up/down)
    /// </summary>
    private void RotateCamera()
    {
        var allowRotate = IsAltOrOptionPressed();
        if (!allowRotate)
            return;

        var leftRight = Input.GetKey(KeyCode.LeftArrow) ? 1 : Input.GetKey(KeyCode.RightArrow) ? -1 : 0;
        var xRotationDirection = 0;
        var yRotationDirection = 0;
        var zRotationDirection = 0;

        if (Input.GetKey(KeyCode.LeftControl))
            zRotationDirection = leftRight;
        else
        {
            yRotationDirection = leftRight;

            if (Input.GetKey(KeyCode.UpArrow))
                xRotationDirection = 1;
            else if (Input.GetKey(KeyCode.DownArrow))
                xRotationDirection = -1;
        }

        arCamera.transform.Rotate(RotationAngle(xRotationDirection), RotationAngle(yRotationDirection), RotationAngle(zRotationDirection));

        float RotationAngle(float rotationDelta)
        {
            return rotationDelta * arrowKeyRotationDegreesPerSecond * Time.deltaTime;
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
            var zOffset = mouseScrollMetersPerUpdate * forwardDelta;
            $"mouseScrollMetersPerUpdate: {mouseScrollMetersPerUpdate} * forwardDelta: {forwardDelta} = zOffset: {zOffset}".Log();
            arCamera.transform.position += new Vector3(x: 0, y: 0, z: zOffset);
        }
    }
}
