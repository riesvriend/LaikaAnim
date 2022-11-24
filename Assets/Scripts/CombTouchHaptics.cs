using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombTouchHaptics : MonoBehaviour
{
    public void OnSkinTouchEnter()
    {
        OVRInput.SetControllerVibration(
            frequency: 0.05f,
            amplitude: 0.3f,
            OVRInput.Controller.RTouch
        );
        //OVRInput.SetControllerVibration(
        //    frequency: 0.5f,
        //    amplitude: 0.5f,
        //    OVRInput.Controller.LTouch
        //);
    }

    public void OnSkinTouchExit()
    {
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
    }
}
