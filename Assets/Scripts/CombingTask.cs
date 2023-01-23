using PowerPetsRescue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CombingStatus : BaseStatus
{
    public const int StrokesPerGoldMedal = 20;
    public int strokeCount;

    public override float Percentage
    {
        get { return ((float)strokeCount / StrokesPerGoldMedal) * 100f; }
    }

    public override void UpdateProgress(ProgressModel progress)
    {
        base.UpdateProgress(progress);

        progress.TaskProgress.ProgressBarText = "Combing";
    }

    public override void IncrementProgress()
    {
        strokeCount++;
    }

    public override void ResetProgress()
    {
        strokeCount = 0;
    }

    public override bool IsCompleted()
    {
        return strokeCount >= StrokesPerGoldMedal;
    }
}

public class CombingTask : BaseTask<CombingStatus>
{
    protected override void OnTaskCompleted()
    {
        game.TaskCompleted(this);
    }

    protected override void AddAnyNewAnimals(List<AnimalInstance> animals)
    {
        foreach (var animal in animals)
            if (animal.animalDef.CanBeCombed && !taskStatus.ContainsKey(animal))
                taskStatus.Add(animal, new CombingStatus { animal = animal, strokeCount = 0 });
    }
}
