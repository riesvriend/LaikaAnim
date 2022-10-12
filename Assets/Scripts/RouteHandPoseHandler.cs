using MalbersAnimations;
using MalbersAnimations.Controller;
using MalbersAnimations.Controller.AI;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// metadata of the animals that we play with
public class AnimalDef
{
    public GameObject gameObject;
    public int index;
    public MAnimalAIControl ai;
    public MAnimal mAnimal;
    public float minComeCloseDistanceFromPlayerInMeter;
    public float animalDistanceFromCameraInMeter;

    public bool IsTableVisible { get; set; }
}

public class RouteHandPoseHandler : MonoBehaviour
{
    // Linked to Transform Hook on center eye camera https://www.youtube.com/watch?v=wxhej5QIrDE
    public TransformReference player;

    // Set at runtime by the PlaygroundInput class
    public AnimalDef animalDef;

    public MWayPoint sendAwayWaypoint;

    public MWayPoint playerWaypoint;


    private void Update()
    {
        if (OVRInput.Get(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            HandleCome();
        }
        if (OVRInput.Get(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            HandleGoAway();
        }
    }


    /// <summary>
    /// The animal should come to the user when the palm faces the user and the fingers
    /// are flexed, and the user faces the animal.
    /// </summary>
    public void HandleCome()
    {
        if (!IsRoutingEnabled)
            return;
       
        Debug.Log("Come");

        if (player == null)
        {
            Debug.Log("HandleCome needs a player");
            return;
        }

        var playerTransform = player.Value;

        // Keep the middle of the horse out of the eyes of the player
        var target = playerTransform.transform.position + (playerTransform.forward * animalDef.minComeCloseDistanceFromPlayerInMeter);

        playerWaypoint.transform.position = target;

        animalDef.ai.SetTarget(playerWaypoint.transform);
    }

    public void HandleComeUnselected()
    {
        Debug.Log("Come Unselected");
    }

    public void HandleGoAway()
    {
        Debug.Log("HandleGoAway");
        if (!IsRoutingEnabled)
            return;

        //var playerTransform = player.Value;
        //var playerPosition = playerTransform.position;

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

        animalDef.ai.SetTarget(sendAwayWaypoint.transform);
    }

    public void HandleGoAwayUnselected()
    {
        Debug.Log("HandleGoAwayUnselected");
    }

    // Can the animal be routed to come and go away? Only if it has an AI and its not on the petting table
    public bool IsRoutingEnabled { get => animalDef?.ai != null && !animalDef.IsTableVisible; }
}
