using Oculus.Interaction;
using PowerPetsRescue;
using Synchrony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LungingStatus : BaseStatus
{
    public const int StepsPerGoldMedal = 200;
    public int stepCount;

    public override float Percentage
    {
        get { return ((float)stepCount / StepsPerGoldMedal) * 100f; }
    }

    public override void UpdateProgress(ProgressModel progress)
    {
        base.UpdateProgress(progress);

        progress.TaskProgress.ProgressBarText = "Lunging";
    }

    public override void IncrementProgress()
    {
        stepCount++;
    }

    public override void ResetProgress()
    {
        stepCount = 0;
    }

    public override bool IsCompleted()
    {
        return stepCount >= StepsPerGoldMedal;
    }
}

public class LungingTask : BaseTask<LungingStatus>
{
    private RayInteractable floor;
    private IPointableElement pointableFloor;

    protected override void OnTaskCompleted()
    {
        game.TaskCompleted(this);
    }

    protected override void AddAnyNewAnimals(List<AnimalInstance> animals)
    {
        foreach (var animal in animals)
            if (animal.animalDef.CanBeCombed && !taskStatus.ContainsKey(animal))
                taskStatus.Add(animal, new LungingStatus { animal = animal, stepCount = 0 });
    }

    public override void Start()
    {
        base.Start();

        floor = game.playground.floorInteractableView.GetComponent<RayInteractable>();
        floor.PointableElement.WhenPointerEventRaised += Floor_WhenPointerEventRaised;

        // Allow raycasting to the floor to point the direction to the horse
        floor.enabled = true;
    }

    private void Floor_WhenPointerEventRaised(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                var interactor = evt.Data as MonoBehaviour; // this is the <Controller/Hand>GrabInteractor
                var rayPose = evt.Pose;
                rayPose.position.ToString().Log();
                var animal = game.FirstAnimal;
                animal.ai.SetDestination(newDestination: rayPose.position, move: true);
                break;

            //case PointerEventType.Unselect:
            //    OnCombingStopped();
            //    SetActiveController(OVRInput.Controller.None);
            //    break;
        }
    }

    public override void Stop()
    {
        floor.enabled = false;
        floor.PointableElement.WhenPointerEventRaised -= Floor_WhenPointerEventRaised;

        base.Stop();
    }
}
