using MalbersAnimations.Controller;
using MalbersAnimations.Controller.AI;
using MalbersAnimations.Utilities;
using System;
using System.Linq;
using UnityEngine;

// Template/prefab for an animal
public class AnimalDef
{
    public const string Horse = "Paard";
    public const string Snowdog = "Laika";
    public const string Rabbit = "Konijn";
    public const string WolfPuppy = "Puppy";
    public const string Elephant = "Olifant";

    public string name;
    public GameObject templateGameObject;
    public float minComeCloseDistanceFromPlayerInMeter;
    public float animalDistanceFromCameraInMeter;

    public bool CanBeBrushed;
    public bool EatsApples;

    public AnimalInstance InstantiateAnimal()
    {
        var animalGameObject = GameObject.Instantiate(original: templateGameObject);

        animalGameObject.SetActive(true); // This flows back into the original...
        templateGameObject.SetActive(false); // So we need to disable it here

        AnimalInstance animal;
        switch (name)
        {
            case Horse:
            case Snowdog:
            case WolfPuppy:
            case Elephant:
                animal = new AnimalInstance();
                break;
            case Rabbit:
                animal = new RabbitInstance();
                break;
            default:
                throw new NotImplementedException();
        }
        animal.gameObject = animalGameObject;
        animal.animalDef = this;

        animal.RandomizeAppearance();

        return animal;
    }

    public MAnimal mAnimal { get => templateGameObject.GetComponent<MAnimal>(); }
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

    protected MaterialChanger MaterialChanger
    {
        get
        {
            var materialChanger = Appearance?.GetComponent<MaterialChanger>();
            return materialChanger;
        }
    }

    protected GameObject Appearance
    {
        get
        {
            var internalComponentsChild = gameObject.transform.GetChild(0);
            var appearance = internalComponentsChild.Find("Appearance");
            return appearance.gameObject;
        }
    }

    protected BlendShape BlendShape
    {
        get
        {
            var blendShape = Appearance?.GetComponent<BlendShape>();
            return blendShape;
        }
    }


    protected MaterialItem Material(string name)
    {
        return MaterialChanger.materialList.Find(item => item.Name == name);
    }

    public virtual void RandomizeAppearance()
    {

    }

    public void Dispose()
    {
        if (gameObject != null)
        {
            GameObject.Destroy(gameObject);
            gameObject = null;
        }
    }
}

public class RabbitInstance : AnimalInstance
{
    public override void RandomizeAppearance()
    {
        base.RandomizeAppearance();

        var skin = Material("Skin");
        // Pick a random skin
        var skinIndex = UnityEngine.Mathf.FloorToInt(UnityEngine.Random.value * skin.materials.Length);
        skin.ChangeMaterial(skinIndex);

        // Set to random for now
        //BlendShape.blendShapes.
    }
}
