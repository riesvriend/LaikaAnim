using MalbersAnimations.Controller;
using PowerPetsRescue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FeedingTask : BaseTask<FeedingStatus>
{
    protected override void OnTaskCompleted()
    {
        game.TaskCompleted(this);
    }

    protected override void AddAnyNewAnimals(List<AnimalInstance> animals)
    {
        foreach (var animal in animals)
            if (animal.animalDef.EatsApples && !taskStatus.ContainsKey(animal))
                taskStatus.Add(animal, new FeedingStatus { animal = animal, bitesCount = 0 });
    }
}

// Nongeneric base task allowing us to list/find the current task in the game
public abstract class PPRBaseTask : MonoBehaviour
{
    protected abstract void AddAnyNewAnimals(List<AnimalInstance> animal);
}

public abstract class BaseTask<TStatus> : PPRBaseTask where TStatus : BaseStatus
{
    // Parent game that uses this task
    public GameInstance game;
    public GameObject GrabbableObject { get; internal set; }
    private GrabbableObjectHaptics haptics = null;

    private bool isUserHoldingGrabbableObject = false;

    // Track progress
    protected Dictionary<AnimalInstance, TStatus> taskStatus = new();

    internal void SyncAnimals(List<AnimalInstance> animals)
    {
        // Delete the status for animals no longer in the game
        foreach (var animal in taskStatus.Keys.ToList())
            if (!animals.Contains(animal))
                taskStatus.Remove(animal);

        AddAnyNewAnimals(animals);
    }

    public AnimalInstance ActiveAnimal
    {
        get => game.activeAnimal;
        protected set { game.SetActiveAnimal(value); }
    }

    private void Start()
    {
        if (GrabbableObject != null)
        {
            haptics = GrabbableObject.GetComponentInChildren<GrabbableObjectHaptics>();
            haptics.OnStrokingStarted.AddListener(OnStrokingStarted);
            haptics.OnStrokingStopped.AddListener(OnStrokingStopped);

            haptics.OnItemGrabbed.AddListener(OnItemGrabbed);
            haptics.OnItemGrabReleased.AddListener(OnItemGrabReleased);
        }
    }

    /// <summary>
    /// We dont rely on Destroy to prevent that we can access all the
    /// related objects
    /// </summary>
    public void Stop()
    {
        // needed to prevent Update() from re-activating the progress model again
        ActiveAnimal = null;
        if (haptics != null)
        {
            haptics.OnStrokingStarted.RemoveListener(OnStrokingStarted);
            haptics.OnStrokingStopped.RemoveListener(OnStrokingStopped);
            haptics.OnItemGrabbed.RemoveListener(OnItemGrabbed);
            haptics.OnItemGrabReleased.RemoveListener(OnItemGrabReleased);
        }
    }

    ProgressModel ProgressModel
    {
        get => game.playground.plankUI.ProgressModel;
    }

    private void Update()
    {
        var progress = ProgressModel;
        var status = ActiveStatus();
        status?.UpdateProgress(progress);
    }

    AnimalInstance FindAnimalInstance(GameObject animal)
    {
        return taskStatus.Keys.SingleOrDefault(a => a.gameObject == animal);
    }

    TStatus ActiveStatus()
    {
        if (ActiveAnimal == null || !isUserHoldingGrabbableObject)
            return null;

        taskStatus.TryGetValue(ActiveAnimal, out var status);
        return status;
    }

    protected abstract void OnTaskCompleted();

    private void OnStrokingStarted(GrabbableObjectHaptics.StrokeEvent e)
    {
        ActiveAnimal = FindAnimalInstance(e.Animal.gameObject);

        var status = ActiveStatus();
        status.IncrementProgress();

        if (status.IsCompleted())
        {
            status.ResetProgress();
            OnTaskCompleted();
        }
    }

    private void OnStrokingStopped(GrabbableObjectHaptics.StrokeEvent e) { }

    private void OnItemGrabReleased()
    {
        isUserHoldingGrabbableObject = false;
    }

    private void OnItemGrabbed(GrabbableObjectHaptics.GrabItemEvent arg0)
    {
        isUserHoldingGrabbableObject = true;
    }
}

public abstract class BaseStatus
{
    public AnimalInstance animal;

    public abstract float Percentage { get; }

    public abstract void IncrementProgress();
    public abstract void ResetProgress();

    public abstract bool IsCompleted();

    public virtual void UpdateProgress(ProgressModel progress)
    {
        // Show stroke progress and target on menu
        progress.TaskTitle = "Voortgang";
        progress.Percentage = Percentage;
    }
}

public class FeedingStatus : BaseStatus
{
    public const int BitesPerGoldMedal = 20;
    public int bitesCount;

    public override float Percentage
    {
        get { return ((float)bitesCount / BitesPerGoldMedal) * 100f; }
    }

    public override void IncrementProgress()
    {
        bitesCount++;
    }

    public override void ResetProgress()
    {
        bitesCount = 0;
    }

    public override bool IsCompleted()
    {
        return bitesCount >= BitesPerGoldMedal;
    }

    public override void UpdateProgress(ProgressModel progress)
    {
        base.UpdateProgress(progress);

        progress.ProgressBarTitle = "Voeren";
    }
}
