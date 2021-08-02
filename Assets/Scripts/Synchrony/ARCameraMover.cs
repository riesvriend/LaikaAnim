using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Synchrony;

/// <summary>
/// Allows moving the AR Camera while in Play mode in the Editor
/// </summary>
public class ARCameraMover : MonoBehaviour
{
    [SerializeField] float mouseScrollScale = 5f;

    private Camera arCamera;

    // Start is called before the first frame update
    void Start()
    {
        arCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!SynchronyUtils.IsPlayingInEditor())
            return;

        // TODO: Move left/right & up/down by moving mouse

        // Move forward/backward by scrolling mouse wheel
        var mouseScrollDeltaVector = Input.mouseScrollDelta; // This is + or - Time.DeltaTime 
        if (mouseScrollDeltaVector != Vector2.zero)
            $"{mouseScrollDeltaVector}".Log();
        var forwardDelta = mouseScrollDeltaVector.y;

        // move back
        var zOffset = mouseScrollScale * forwardDelta;
        arCamera.transform.position += new Vector3(x: 0, y: 0, z: zOffset);
        $"Z: {arCamera.transform.position.z}".Log();
    }
}
