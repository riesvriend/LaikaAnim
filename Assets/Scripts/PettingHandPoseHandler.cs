using MalbersAnimations;
using MalbersAnimations.Controller.AI;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PettingHandPoseHandler : MonoBehaviour
{
    [SerializeField]
    private LaikaMovement laikaMovementHandler;

    // Linked to Transform Hook on center eye camera https://www.youtube.com/watch?v=wxhej5QIrDE
    public TransformReference player;

    public MAnimalAIControl animalAI;

    public MWayPoint sendAwayWaypoint;

    public MWayPoint playerWaypoint;

#if UNITY_EDITOR

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            HandleCome();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            HandleGoAway();
        }
    }
#endif


    /// <summary>
    /// The animal should come to the user when the palm faces the user and the fingers
    /// are flexed, and the user faces the animal.
    /// </summary>
    public void HandleCome()
    {
        Debug.Log("Come");

        if (player == null)
        {
            Debug.Log("HandleCome needs a player");
            return;
        }

        var playerTransform = player.Value;

        // Keep the middle of the horse out of the eyes of the player
        var target = playerTransform.transform.position + (playerTransform.forward * 1.0f);

        playerWaypoint.transform.position = target;

        animalAI.SetTarget(playerWaypoint.transform);

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

        var playerTransform = player.Value;
        var playerPosition = playerTransform.position;

        //var radius = 4.0f;

        //// X = opposite side of the field relative to the player
        //var x = radius;
        //if (playerPosition.x > 0)
        //    x = -radius;

        //// Z is same side of player so horse looks you in the eye sideways
        //var z = radius;
        //if (playerPosition.z < 0)
        //    z = -radius;

        //var target = new Vector3(x, 0, z);

        //animalAI?.SetDestination(target);

        if (sendAwayWaypoint == null)
        {
            Debug.Log("HandleGoAway needs a sendAwayWaypoint");
            return;
        }
        animalAI.SetTarget(sendAwayWaypoint.transform);
    }

    public void HandleGoAwayUnselected()
    {
        Debug.Log("HandleGoAwayUnselected");

        //laikaMovementHandler?.HandleVoiceCommand("sit");
    }



}
