using MalbersAnimations.Controller;
using MalbersAnimations.Controller.AI;
using MalbersAnimations.Utilities;
using System;
using System.Linq;
using UnityEngine;

// Template/prefab for an animal
public class AnimalDef : MonoBehaviour
{
    public string DisplayName;
    public GameObject templateGameObject;
    public float minComeCloseDistanceFromPlayerInMeter;
    public float animalDistanceFromCameraInMeter;

    public float MaxFeedingScale;
    public bool CanBeCombed;
    public bool EatsApples;

    public MAnimal mAnimal
    {
        get => templateGameObject.GetComponent<MAnimal>();
    }
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
    public MAnimal mAnimal
    {
        get => gameObject.GetComponent<MAnimal>();
    }

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

    public virtual void RandomizeAppearance() { }

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
        var skinIndex = UnityEngine.Mathf.FloorToInt(
            UnityEngine.Random.value * skin.materials.Length
        );
        if (skinIndex >= skin.materials.Length)
            // Random.value is inclusive of 1.0 this is a safety net
            skinIndex = skin.materials.Length - 1;
        skin.ChangeMaterial(skinIndex);
    }
}

public class RabbitPuppyInstance : RabbitInstance { }
