// metadata of the games and animals in the main menu
using MalbersAnimations;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Assets.Scripts;
using PowerPetsRescue;
using MalbersAnimations.Utilities;
using MalbersAnimations.Controller;

public class GameDef
{
    public string name;
    public Type GameType; // A GameInstance or derived type

    //*** If its to freeplay with an animal set a gameobject
    public List<AnimalDef> animals = new List<AnimalDef>();

    public bool IsTableVisible { get; set; }
    public bool IsAppleVisible { get; set; }
    public bool IsCombVisible { get; set; }
}

public class GameInstance : MonoBehaviour
{
    public GameDef gameDef;
    public PlaygroundInput playground;

    public List<AnimalInstance> animals = new List<AnimalInstance>();
    public AnimalInstance activeAnimal { get; private set; }

    protected BrushingTask brushingTask = null;

    // Dont call this 'Start' as that collides with a Unity event name
    public virtual void StartGame()
    {
        playground.pettingHandPoseHandler.game = this;

        playground.table.SetActive(gameDef.IsTableVisible);
        playground.apple.SetActive(gameDef.IsAppleVisible);
        playground.comb.SetActive(gameDef.IsCombVisible);

        if (gameDef.IsCombVisible)
        {
            brushingTask = gameObject.AddComponent<BrushingTask>();
            brushingTask.Comb = playground.comb;
            brushingTask.game = this;
        }

        foreach (var animalDef in gameDef.animals)
            AddAnimal(animalDef);
    }

    private void Update()
    {
        var ai = activeAnimal?.ai;
        if (ai == null)
            return;

        // Workaround: clear the animals target once it has arrived there
        // this way we prevent the animals from running back to the target if one of the animals is re-selected to come back to the player
        if (!ai.ActiveAgent && ai.Target != null)
            ai.Target = null;
    }

    private AnimalInstance AddAnimal(AnimalDef animalDef)
    {
        var animal = animalDef.InstantiateAnimal();
        animals.Add(animal);

        SetActiveAnimal(animal);

        if (brushingTask != null && animal.animalDef.CanBeBrushed)
            brushingTask.AddAnimal(animal);

        PositionAnimal(animal);

        // Send the new animal to the player (in case its dropped to far away)
        // playground.pettingHandPoseHandler.HandleCome();

        return animal;
    }

    public void SetActiveAnimal(AnimalInstance animal)
    {
        activeAnimal = animal;
    }

    void PositionAnimal(AnimalInstance animal)
    {
        if (animal?.ai != null)
        {
            // Prevent it to start running off directly and running through the player
            animal.ai.Stop();
            animal.ai.ClearTarget();
        }

        var camera = playground.cameraOrEyeTransform;

        var forward = camera.forward;
        // Forward from camera needs to be projected to the horizontal plane
        forward.y = 0f;
        forward = forward.normalized;
        if (gameDef.IsTableVisible)
            // When sitting at the table, try to align the virtual table with the real table by disregarding camera/head
            // and assume the world view is reset to face the table
            forward = Vector3.forward;

        var animalPos = camera.position;
        if (animal != null)
        {
            animalPos += forward * animal.animalDef.animalDistanceFromCameraInMeter;
        }
        Quaternion animalRotation = Quaternion.identity;
        Transform animalTransform = camera;
        if (animal != null)
        {
            // Animal is on the floor (0), unless its on the table
            animalPos.y = 0f;

            // https://stackoverflow.com/questions/22696782/placing-an-object-in-front-of-the-camera
            var animalYRotation = new Quaternion(
                0.0f,
                camera.transform.rotation.y,
                0.0f,
                camera.transform.rotation.w
            ).eulerAngles;
            if (gameDef.IsTableVisible)
                animalYRotation = forward;

            animalRotation = Quaternion.Euler(
                animalYRotation.x + 180,
                animalYRotation.y,
                animalYRotation.z + 180
            );

            animalTransform = animal.gameObject.transform;
            animalTransform.position = animalPos;
            animalTransform.rotation = animalRotation;
        }

        if (gameDef.IsTableVisible)
        {
            var tablePos = camera.position + forward * playground.tableDistanceFromCameraInMeter;

            // put the table-top below the camera
            // TODO: instead of hardcoding the 0.6 meter between eyes and table top, consider
            // measuring the height of the lowest hand/controller and presume this to be the user's table height
            // or, give the user a control mechanism to move the virtual table up or down
            tablePos.y = -0.11f; // 85cm - 11 = 74cm, a common table.

            playground.table.transform.position = tablePos;
            playground.table.transform.rotation = animalRotation;

            // Move the animal up, onto the table
            var animalHeightOnTableTop = Math.Max(
                0f,
                tablePos.y
                    + playground.tableHeight
                    + 0.2f /* margin */
                    + 0.8f // 80cm above the table to create a drop from the sky effect
            );
            if (animalTransform != null)
                animalTransform.position = new Vector3(
                    animalPos.x,
                    animalHeightOnTableTop,
                    animalPos.z
                );
        }

        // Place the apple 90 degrees from the animal (to the side of the animal)
        // TODO: only do this if this is the first animal; or add a new apple for each animal
        playground.apple.transform.position =
            animalTransform.position - Quaternion.AngleAxis(140, Vector3.up) * forward * -0.4f;
        playground.comb.transform.position =
            playground.apple.transform.position + Vector3.up * 0.3f; // Drop the comb on the apple
    }

    /// <summary>
    /// We dont rely on Destroy to prevent that we can access all the
    /// related objects
    /// </summary>
    public void Stop()
    {
        if (brushingTask != null)
            brushingTask.Stop();
    }

    private void OnDestroy()
    {
        if (brushingTask != null)
            Destroy(brushingTask);

        while (animals.Count > 0)
        {
            var animal = animals[0];
            animals.RemoveAt(0);
            animal.Dispose();
        }
    }

    public void SetTarget(IWayPoint wayPoint)
    {
        var ai = activeAnimal?.ai;
        if (ai == null)
            return;
        ai.SetTarget(wayPoint.transform);
    }

    ProgressModel ProgressModel
    {
        get => playground.plankUI.ProgressModel;
    }

    internal void TaskCompleted(BrushingTask brushingTask)
    {
        playground.PlaySoundTaskCompleted();
        ProgressModel.AddMedal();
        AddAnimal(animals.First().animalDef);
    }

    // Hack
    public AnimalInstance firstAnimal
    {
        get => animals.FirstOrDefault();
    }
}

public class JustShowTheAnimalGame : GameInstance
{
    public override void StartGame()
    {
        base.StartGame();
    }
}

public class HomeScreenGame : GameInstance
{
    public override void StartGame()
    {
        base.StartGame();
    }
}

public class StationaryGame : GameInstance
{
    public override void StartGame()
    {
        base.StartGame();

        /*

            1.	x Optie: De controller trilt als je borstelt
            2.	~ Toon een progress bar voor het borstelen op de menuplank
            3.	x Klaar met borstelen, een nieuwe konijn in random kleur
            4.	x Klaar met borstelen, een nieuwe konijn in dezelfde kleur
            5.	(later) En een gouden ster
            6.	Voer de appel
            7.	Mjom mjom
            8.	Gouden ster
            9.	En
          
         */

        // Show "TIP: start brushing your puppy!" on the menu plank


        // Also play a voice that says the same

        // Show brush progress: 0% on the menu

        // On brushing, increase the brusing progress bar
    }
}

public class RoomScaleGame : GameInstance
{
    public override void StartGame()
    {
        base.StartGame();
    }
}
