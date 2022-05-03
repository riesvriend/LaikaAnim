using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokePetting : MonoBehaviour
{
    private LaikaMovement laikaMovementHandler;

    private void Awake()
    {
        this.laikaMovementHandler = FindObjectOfType<LaikaMovement>();
    }

    /// <summary>
    /// When the hand first touches the animal
    /// </summary>
    public void HandleHover()
    {
        Debug.Log("PokePetting HandleHover");

        // Start hacking (hijgen)
        var hackingAudioSource = laikaMovementHandler.gameObject.GetComponentInChildren<AudioSource>();
        hackingAudioSource?.Play();
    }

    public void HandleUnhover()
    {
        Debug.Log("PokePetting HandleUnhover");
        var hackingAudioSource = laikaMovementHandler.gameObject.GetComponentInChildren<AudioSource>();
        hackingAudioSource?.Stop();
    }


    /// <summary>
    /// When the hand does a firm petting (touches the deep surface)
    /// </summary>
    public void HandleSelect()
    {
        Debug.Log("PokePetting HandleSelect");

        laikaMovementHandler?.HandleVoiceCommand("sit");

    }

    public void HandleUnselect()
    {
        Debug.Log("PokePetting HandleUnselect");
        laikaMovementHandler?.HandleVoiceCommand("up");
    }
}
