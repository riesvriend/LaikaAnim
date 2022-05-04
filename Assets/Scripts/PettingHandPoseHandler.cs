using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PettingHandPoseHandler : MonoBehaviour
{
    [SerializeField]
    private LaikaMovement laikaMovementHandler;
        
    /// <summary>
    /// The animal should come to the user when the palm faces the user and the fingers
    /// are flexed, and the user faces the animal.
    /// </summary>
    public void HandleCome()
    {
        Debug.Log("Come");

        //laikaMovementHandler?.HandleVoiceCommand("sit");

    }

    public void HandleComeUnselected()
    {
        Debug.Log("Come Unselected");

        //laikaMovementHandler?.HandleVoiceCommand("sit");
    }

    public void HandleGoAway()
    {
        Debug.Log("HandleGoAway");

        //laikaMovementHandler?.HandleVoiceCommand("sit");

    }

    public void HandleGoAwayUnselected()
    {
        Debug.Log("HandleGoAwayUnselected");

        //laikaMovementHandler?.HandleVoiceCommand("sit");
    }



}
