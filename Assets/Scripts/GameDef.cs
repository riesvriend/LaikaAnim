// metadata of the games and animals in the main menu
using MalbersAnimations;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Assets.Scripts;

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

    protected BrushingTask brushingTask = null;

    // Dont call this 'Start' as that collides with a Unity event name
    public virtual void StartGame()
    {
        playground.pettingHandPoseHandler.game = this;

        playground.table.SetActive(gameDef.IsTableVisible);
        playground.apple.SetActive(gameDef.IsAppleVisible);
        playground.comb.SetActive(gameDef.IsCombVisible);

        if (gameDef.IsCombVisible)
            brushingTask = gameObject.AddComponent<BrushingTask>();

        foreach (var animalDef in gameDef.animals)
        {
            var animal = animalDef.InstantiateAnimal();
            animals.Add(animal);

            if (brushingTask != null && animal.animalDef.CanBeBrushed)
                brushingTask.animalStrokingStatus.Add(
                    animal,
                    new StrokingStatus { animal = animal, strokeCount = 0 }
                );
        }

        foreach (var animal in animals)
            // For now this only works properly for a single animal
            PositionAnimal(animal);
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
            animalPos += forward * animal.animalDef.animalDistanceFromCameraInMeter;
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
            tablePos.y = -0.11f; // 85cm - 11 = 74cm, a common table  // Math.Min(0f, cameraOrEyeTransform.position.y - tableHeight - 0.45f);

            playground.table.transform.position = tablePos;
            playground.table.transform.rotation = animalRotation;

            // Move the animal up, onto the table
            var animalHeightOnTableTop = Math.Max(
                0f,
                tablePos.y + playground.tableHeight + 0.2f /* margin */
            );
            if (animalTransform != null)
                animalTransform.position = new Vector3(
                    animalPos.x,
                    animalHeightOnTableTop,
                    animalPos.z
                );
        }

        // Place the apple 90 degrees from the animal (to the side of the animal)
        playground.apple.transform.position =
            animalTransform.position - Quaternion.AngleAxis(140, Vector3.up) * forward * -0.4f;
        playground.comb.transform.position =
            playground.apple.transform.position + Vector3.up * 0.3f; // Drop the comb on the apple
    }

    private void OnDestroy()
    {
        while (animals.Count > 0)
        {
            var animal = animals[0];
            animals.RemoveAt(0);
            animal.Dispose();
        }
    }

    public void SetTarget(IWayPoint wayPoint)
    {
        firstAnimal.ai.SetTarget(wayPoint.transform);
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

            1.	Optie: De controller trilt als je borstelt
            2.	Toon een progress bar voor het borstelen op de menuplank
            3.	Klaar met borstelen, je eerste gouden ster!
            4.	Konijn
            a.	Voer de appel
            b.	Mjom mjom
            c.	Gouden ster
            d.	En een nieuwe konijn in random kleur
          
         */

        // Show "TIP: start brushing your puppy!" on the menu plan
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
