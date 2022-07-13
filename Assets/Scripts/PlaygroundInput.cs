using Synchrony;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A or X button = Rotate Animal
/// B or Y button = Rotate video
/// </summary>
public class PlaygroundInput : MonoBehaviour
{
    public List<GameObject> animalsToRotate = new List<GameObject>();
    public int activeAnimalIndex = 0;

    private void Awake()
    {
        ActivateActiveAnimal();
    }

    void Update()
    {
        // rotate animal
        if (OVRInput.Get(OVRInput.RawButton.A) || Input.GetKeyDown(KeyCode.P))
        {
            if (activeAnimalIndex >= animalsToRotate.Count - 1)
                activeAnimalIndex = 0;
            else
                activeAnimalIndex += 1;

            ActivateActiveAnimal();
        }
    }

    private void ActivateActiveAnimal()
    {
        var i = 0;
        foreach (var animal in animalsToRotate)
        {
            animal.SetActive(activeAnimalIndex == i);
            i += 1;
        }
    }
}
