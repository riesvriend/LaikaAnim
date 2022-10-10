using PPP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokePetting : MonoBehaviour
{
    private IPettableAnimal pettableAnimal;

    private void Awake()
    {
        this.pettableAnimal = FindObjectOfType<LaikaMovement>();
    }

    /// <summary>
    /// When the hand first touches the animal
    /// </summary>
    public void HandleHover()
    {
        Debug.Log("PokePetting HandleHover");

        // Start hacking (hijgen)
        var hackingAudioSource = pettableAnimal.gameObject.GetComponentInChildren<AudioSource>();
        hackingAudioSource?.Play();
    }

    public void HandleUnhover()
    {
        Debug.Log("PokePetting HandleUnhover");
        var hackingAudioSource = pettableAnimal.gameObject.GetComponentInChildren<AudioSource>();
        hackingAudioSource?.Stop();
    }


    /// <summary>
    /// When the hand does a firm petting (touches the deep surface)
    /// </summary>
    public void HandleSelect()
    {
        Debug.Log("PokePetting HandleSelect");
        pettableAnimal.OnStartPetting();
    }

    public void HandleUnselect()
    {
        Debug.Log("PokePetting HandleUnselect");
        pettableAnimal.OnStopPetting();
    }
}
