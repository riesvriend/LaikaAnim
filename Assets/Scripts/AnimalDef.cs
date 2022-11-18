using MalbersAnimations.Controller;
using MalbersAnimations.Controller.AI;
using System;
using System.Linq;
using UnityEngine;

// Template/prefab for an animal
[CreateAssetMenu(menuName = "Vriend/New Animal Definition", order = -100, fileName = "New Skybox Descriptor")]
public class AnimalDef
{
    public const string Horse = "Paard";
    public const string Snowdog = "Laika";
    public const string Rabbit = "Konijn";
    public const string WolfPuppy = "Puppy";
    public const string Elephant = "Olifant";

    public string name;
    public GameObject gameObject;
    public float minComeCloseDistanceFromPlayerInMeter;
    public float animalDistanceFromCameraInMeter;
    public MAnimal mAnimal { get => gameObject.GetComponent<MAnimal>(); }
}

public class AnimalInstance : IDisposable
{
    public AnimalDef animalDef;
    public GameObject gameObject;
    public MAnimalAIControl ai
    {
        get
        {
            var aiList = gameObject.GetComponentsInChildren<MAnimalAIControl>();
            var ai = aiList.SingleOrDefault();
            return ai;
        }
    }
    public MAnimal mAnimal { get => gameObject.GetComponent<MAnimal>(); }

    public void Dispose()
    {
        if (gameObject != null)
        {
            GameObject.Destroy(gameObject);
            gameObject = null;
        }
    }
}
